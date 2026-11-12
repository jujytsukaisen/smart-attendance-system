using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartAttendance.Infrastructure.Identity.Models;

namespace SmartAttendance.Infrastructure.Identity.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<string> permissions, Guid? companyId = null)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not found.");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found.");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not found.");
        var expirationMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");

        // 👇 DEBUG - احذفها بعد التشخيص
        Console.WriteLine($"[JWT GEN DEBUG] Secret used: '{secret}'");
        Console.WriteLine($"[JWT GEN DEBUG] Secret length: {secret.Length}");
        Console.WriteLine($"[JWT GEN DEBUG] Issuer: '{issuer}'");
        Console.WriteLine($"[JWT GEN DEBUG] Audience: '{audience}'");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.DomainUserId.ToString()), // ← Domain User ID (Mapped to NameIdentifier by default)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.DomainUserId.ToString()), // ← Domain User ID (FK in Notifications)
            new Claim("IdentityUserId", user.Id.ToString()),                     // ← ASP.NET Identity user ID
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var actualCompanyId = companyId ?? user.CompanyId;
        if (actualCompanyId.HasValue)
        {
            claims.Add(new Claim("CompanyId", actualCompanyId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");

        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(randomNumber),
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow
        };
    }
}