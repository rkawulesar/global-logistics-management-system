using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GLMS_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GLMS_API.Controllers;
//Code Attribution for JWT authentication and Web API controller structure
//Links:
//https://www.c-sharpcorner.com/article/jwt-json-web-token-authentication-in-asp-net-core/
//https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-9.0&tabs=visual-studio
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly GlmsContext _context;

    public AuthController(
        IConfiguration configuration,
        GlmsContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto dto)
    {
        // Finds the matching user
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Username == dto.Username);

        // Validates password
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var passwordMatches = false;

        // Old plain-text password support
        if (!user.Password.StartsWith("AQAAAA"))
        {
            passwordMatches = user.Password == dto.Password;
        }
        else
        {
            var hasher = new PasswordHasher<User>();

            var passwordResult = hasher.VerifyHashedPassword(
                user,
                user.Password,
                dto.Password);

            passwordMatches = passwordResult != PasswordVerificationResult.Failed;
        }

        if (!passwordMatches)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        // Generates JWT token
        var token = GenerateJwtToken(user);

        // Returns token and role
        return Ok(new LoginResponseDto
        {
            Token = token,
            Role = user.Role
        });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings =
            _configuration.GetSection("Jwt");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        // JWT claims
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),

            new Claim(ClaimTypes.Role, user.Role),

            new Claim(
                "userId",
                user.Id.ToString()),

            new Claim(
                "clientId",
                user.ClientId?.ToString() ?? ""),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}

// Login request DTO
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

// Login response DTO
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}