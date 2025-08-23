using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CineTicket.Repositories.Interfaces;
using CineTicket.Services.Interfaces;
using CineTicket.Repositories.Implementations;
using CineTicket.Services.Implementations;
using CineTicket.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using CineTicket.Data.Repositories.Interfaces;
using CineTicket.MappingProfiles;
using CineTicket.Models;
using Microsoft.AspNetCore.Identity;
using CineTicket.Data.Repositories.Implementations;
using CineTicket.Services;
using Microsoft.Extensions.FileProviders;
using CineTicket.Models.Momo;
using CineTicket.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddControllers()
    .AddJsonOptions(o => { o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CineTicketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()  // log cả parameter (cẩn thận trong production)
           .EnableDetailedErrors()        // log lỗi SQL chi tiết
           .LogTo(Console.WriteLine, LogLevel.Information) // ghi log ra console
);


builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CineTicketDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/seatHub"))
                {
                    ctx.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500"
            // "https://127.0.0.1:5500",
            // "https://localhost:5500"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

/* -------------------- DI -------------------- */
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IPhimRepository, PhimRepository>();
builder.Services.AddScoped<IPhimService, PhimService>();
builder.Services.AddScoped<ISuatChieuRepository, SuatChieuRepository>();
builder.Services.AddScoped<ISuatChieuService, SuatChieuService>();
builder.Services.AddScoped<IPhongChieuRepository, PhongChieuRepository>();
builder.Services.AddScoped<IPhongChieuService, PhongChieuService>();
builder.Services.AddScoped<IVeRepository, VeRepository>();
builder.Services.AddScoped<IVeService, VeService>();
builder.Services.AddScoped<IGheRepository, GheRepository>();
builder.Services.AddScoped<IGheService, GheService>();
builder.Services.AddScoped<ILoaiPhimRepository, LoaiPhimRepository>();
builder.Services.AddScoped<ILoaiPhimService, LoaiPhimService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBapNuocRepository, BapNuocRepository>();
builder.Services.AddScoped<IBapNuocService, BapNuocService>();
builder.Services.AddScoped<IHoaDonRepository, HoaDonRepository>();
builder.Services.AddScoped<IHoaDonService, HoaDonService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("Momo"));

builder.Services.AddSignalR();          
builder.Services.AddHostedService<SeatHoldExpiryWorker>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();


var app = builder.Build();


var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
};
fwd.KnownNetworks.Clear();
fwd.KnownProxies.Clear();
app.UseForwardedHeaders(fwd);

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images")),
    RequestPath = "/Images"
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowLocalhost");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SeatHub>("/seatHub");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAsync(serviceProvider);
}

app.Run();
