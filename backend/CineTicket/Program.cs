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
using CineTicket.MappingProfiles;  // 👈 thêm dòng này!

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Thêm cấu hình JWT Authentication
builder.Services.AddAuthentication(options =>
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
        )
    };
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<CineTicketDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500") // nếu bạn dùng Live Server bên VS xanh
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



builder.Services.AddAuthorization();

builder.Services.AddScoped<IPhimRepository, PhimRepository>();
builder.Services.AddScoped<IPhimService, PhimService>();
builder.Services.AddScoped<ISuatChieuRepository, SuatChieuRepository>();
builder.Services.AddScoped<ISuatChieuService, SuatChieuService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IPhongChieuRepository, PhongChieuRepository>();
builder.Services.AddScoped<IPhongChieuService, PhongChieuService>();
builder.Services.AddScoped<IVeRepository, VeRepository>();
builder.Services.AddScoped<IVeService, VeService>();
builder.Services.AddScoped<IGheRepository, GheRepository>();
builder.Services.AddScoped<IGheService, GheService>();



var app = builder.Build();

app.UseCors();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Bắt buộc phải có thứ tự: Authentication → Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
