using BCrypt.Net;
using Kolosnycin.Data;
using Kolosnycin.Models;
using Kolosnycin.Models.Auth;
using Kolosnycin.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginRequest = Kolosnycin.Models.Auth.LoginRequest;
using RegisterRequest = Kolosnycin.Models.Auth.RegisterRequest;
using AuthResponse = Kolosnycin.Models.Auth.AuthResponse;


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
        // Проверяем существование пользователя
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return BadRequest(new { error = "Пользователь с таким email уже существует" });
        }

        // Хешируем пароль - правильный синтаксис
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new AppUser
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

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
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive == true);

        if (user == null)
        {
            return Unauthorized(new { error = "Неверный email или пароль" });
        }

        // Проверяем пароль - правильный синтаксис
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            return Unauthorized(new { error = "Неверный email или пароль" });
        }

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