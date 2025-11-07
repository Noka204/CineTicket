// Program.cs
using AutoMapper;
using CineTicket.Data;
using CineTicket.Data.Repositories.Implementations;
using CineTicket.Data.Repositories.Interfaces;
using CineTicket.Hubs;
using CineTicket.Localization;
using CineTicket.MappingProfiles;
using CineTicket.Models;
using CineTicket.Models.Momo;
using CineTicket.Repositories.Implementations;
using CineTicket.Repositories.Interfaces;
using CineTicket.Resources.Localization;
using CineTicket.Services;
using CineTicket.Services.Implementations;
using CineTicket.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

/* ---------------- MVC + JSON + i18n ---------------- */
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
    .AddDataAnnotationsLocalization();

builder.Services.AddLocalization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* ---------------- DB ---------------- */
builder.Services.AddDbContext<CineTicketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           .LogTo(Console.WriteLine, LogLevel.Information));

/* ---------------- Identity + Auth ---------------- */
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CineTicketDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && ctx.HttpContext.Request.Path.StartsWithSegments("/seatHub"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddSingleton<JwtTokenGenerator>();
builder.Services.AddAuthorization();

/* ---------------- CORS ---------------- */
string[] allowedOrigins = new[] { "http://localhost:5500", "http://127.0.0.1:5500" /*, "https://localhost:5500"*/ };

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowLocalhost", p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

/* ---------------- HttpClient: LibreTranslate ---------------- */
builder.Services.AddHttpClient("LibreTranslate", c =>
{
    c.BaseAddress = new Uri("http://127.0.0.1:5000");
    c.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<ILibreTranslateService, LibreTranslateService>();

/* ---------------- DI domain services ---------------- */
builder.Services.AddMemoryCache();
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
builder.Services.AddScoped<IRapService, RapService>();
builder.Services.AddScoped<IRapRepository, RapRepository>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("Momo"));
builder.Services.AddScoped<IVnPayService,VnPayService>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<SeatHoldExpiryWorker>();
builder.Services.AddScoped<IKhuyenMaiService,KhuyenMaiService>();
builder.Services.AddScoped<IKhuyenMaiCodeService,KhuyenMaiCodeService>();
builder.Services.AddScoped<IKhuyenMaiRepository,KhuyenMaiRepository>();
builder.Services.AddScoped<IKhuyenMaiCodeRepository,KhuyenMaiCodeRepository>();



builder.Logging.ClearProviders();
builder.Logging.AddConsole();

/* ---------------- Build ---------------- */
var app = builder.Build();

/* ---------------- Proxy headers (nếu có) ---------------- */
var fwd = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
};
fwd.KnownNetworks.Clear();
fwd.KnownProxies.Clear();
app.UseForwardedHeaders(fwd);

/* ---------------- Localization: đặt sớm ---------------- */
var supported = new[]
{
    new CultureInfo("vi-VN"),
    new CultureInfo("en-US"),
    new CultureInfo("fr-FR")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi-VN"),
    SupportedCultures = supported,
    SupportedUICultures = supported,
    RequestCultureProviders = new IRequestCultureProvider[]
    {
        new SimpleLanguageProvider(),                  // ?lang= / X-Lang
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    }
});

/* ---------------- HTTPS + Swagger ---------------- */
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

/* ---------------- CORS phải trước Auth ---------------- */
app.UseCors("AllowLocalhost");

/* ---------------- Auth ---------------- */
app.UseAuthentication();
app.UseAuthorization();

/* ---------------- Static files (1 lần là đủ) ---------------- */
// Nếu ảnh nằm trong wwwroot/Images → chỉ cần thế này:
app.UseStaticFiles();

// Nếu bạn thực sự muốn set ACAO cho static (không khuyến nghị khi đã dùng <img src> trực tiếp):
// app.UseStaticFiles(new StaticFileOptions {
//     OnPrepareResponse = ctx => {
//         var origin = ctx.Context.Request.Headers["Origin"].ToString();
//         if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
//             ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = origin; // 1 origin duy nhất
//         ctx.Context.Response.Headers["Vary"] = "Origin"; // tránh cache chéo
//     }
// });

/* ---------------- (Optional) route chẩn đoán i18n ---------------- */
app.MapGet("/__i18n/diag", (HttpContext http, IStringLocalizer<SharedResource> L) =>
{
    var lang = http.Request.Query["lang"].ToString();
    if (!string.IsNullOrWhiteSpace(lang))
    {
        try
        {
            var ci = CultureInfo.GetCultureInfo(lang);
            CultureInfo.CurrentCulture = ci;
            CultureInfo.CurrentUICulture = ci;
        }
        catch { }
    }
    var hero = L["Hero_Title"];
    var asm = typeof(SharedResource).Assembly;
    return Results.Json(new
    {
        CurrentUICulture = CultureInfo.CurrentUICulture.Name,
        LocalizerNotFound = hero.ResourceNotFound,
        LocalizerValue = hero.Value,
        Embedded = asm.GetManifestResourceNames()
    });
});

/* ---------------- Hubs + Controllers ---------------- */
app.MapHub<SeatHub>("/seatHub");
app.MapControllers();

/* ---------------- Seed roles ---------------- */
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAsync(sp);
}

app.Run();
