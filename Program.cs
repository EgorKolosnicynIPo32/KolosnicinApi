using Kolosnycin.Data;
using Kolosnycin.Models;
using Kolosnycin.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Kolosnycin.Data;
using Kolosnycin.Models;
using Kolosnycin.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем CORS для работы с фронтендом
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7268",  // Порт вашего фронтенда
            "http://localhost:7268",
            "https://localhost:5001",
            "http://localhost:5000",
            "https://localhost:7000",
            "http://localhost:5023"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Добавляем контроллеры
builder.Services.AddControllers();

// Настройка JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JWT settings not configured properly");
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Добавляем DbContext для PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем сервисы
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

// Используем CORS
app.UseCors("AllowFrontend");

// Для работы со статическими файлами из папки wwwroot
app.UseDefaultFiles();  // Ищет index.html по умолчанию
app.UseStaticFiles();   // Разрешает отдавать статические файлы

// Перенаправление на index.html при открытии корня
app.MapGet("/", () => Results.Redirect("/index.html"));

// ВАЖНО: Отключаем HTTPS редирект для разработки, чтобы не было проблем с CORS
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Тестовый endpoint для проверки работы API
app.MapGet("/api/test", () => Results.Ok(new
{
    message = "API is working",
    status = "ok",
    time = DateTime.Now,
    endpoints = new
    {
        login = "/api/auth/login",
        register = "/api/auth/register",
        customers = "/api/customer"
    }
}));

app.Run();