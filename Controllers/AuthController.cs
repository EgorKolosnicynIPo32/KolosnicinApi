using BCrypt.Net;
using Kolosnycin.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kolosnycin.Data;
using Kolosnycin.Models;
using Kolosnycin.Models.Auth;
using Kolosnycin.Services;

namespace Kolosnycin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _tokenService;

    public AuthController(AppDbContext context, IJwtTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        // Проверяем, существует ли пользователь
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return BadRequest(new { error = "Пользователь с таким email уже существует" });
        }

        // Создаем нового пользователя в соответствии со структурой app_users
        var user = new AppUser
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Генерируем токен
        var token = _tokenService.GenerateToken(user);

        var response = new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Address = user.Address,
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(60)
        };

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        // Ищем пользователя по email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive == true);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { error = "Неверный email или пароль" });
        }

        // Генерируем токен
        var token = _tokenService.GenerateToken(user);

        var response = new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Address = user.Address,
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(60)
        };

        return Ok(response);
    }
}