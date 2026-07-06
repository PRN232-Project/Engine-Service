using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.API.Models.Common;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/auth")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json", "application/xml")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly LmsDbContext _dbContext;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthController(IConfiguration configuration, LmsDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.ValidationFail(ModelState));

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == request.Username);
        if (user == null) return Unauthorized(ApiResponse<object>.Fail("Invalid credentials"));

        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed) return Unauthorized(ApiResponse<object>.Fail("Invalid credentials"));

        var refresh = Guid.NewGuid().ToString("N");
        user.RefreshToken = refresh;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _dbContext.SaveChangesAsync();

        var token = CreateToken(user.Username, user.Role, TimeSpan.FromHours(1));
        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse { AccessToken = token, RefreshToken = refresh, ExpiresIn = 3600 }, "Login successful"));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.ValidationFail(ModelState));

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken);
        if (user == null || user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Unauthorized(ApiResponse<object>.Fail("Invalid or expired refresh token"));

        var newRefresh = Guid.NewGuid().ToString("N");
        user.RefreshToken = newRefresh;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _dbContext.SaveChangesAsync();

        var token = CreateToken(user.Username, user.Role, TimeSpan.FromHours(1));
        return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse { AccessToken = token, RefreshToken = newRefresh, ExpiresIn = 3600 }, "Token refreshed successfully"));
    }

    private string CreateToken(string username, string role, TimeSpan lifetime)
    {
        var key = _configuration["Jwt:Key"] ?? "DEV_ONLY_CHANGE_ME_1234567890123456";
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.Add(lifetime), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
