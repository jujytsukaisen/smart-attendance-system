using System;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Infrastructure.Identity.Models;
using SmartAttendance.Infrastructure.Persistence;

namespace SmartAttendance.Infrastructure.Identity.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ApplicationDbContext _dbContext;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dbContext = dbContext;
    }

    public async Task<AuthenticationResponse> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            await _userManager.AccessFailedAsync(user);
            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new UnauthorizedAccessException("User is locked out.");
            }
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        
        // Load claims to extract permissions
        var userClaims = await _userManager.GetClaimsAsync(user);
        var permissions = userClaims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

        // Also add role claims
        foreach (var role in roles)
        {
            var roleEntity = await _roleManager.FindByNameAsync(role);
            if (roleEntity != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                permissions.AddRange(roleClaims.Where(rc => rc.Type == "Permission").Select(rc => rc.Value));
            }
        }
        
        permissions = permissions.Distinct().ToList();

        var token = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return new AuthenticationResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt
        };
    }

    public async Task<string> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        return user.Id.ToString();
    }

    public async Task<AuthenticationResponse> RefreshTokenAsync(string token)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .Include(r => r.User)
            .SingleOrDefaultAsync(r => r.Token == token);

        if (refreshToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        if (!refreshToken.IsActive)
        {
            // Token Replay Detected! Revoke all descendant tokens
            var activeTokens = await _dbContext.RefreshTokens
                .Where(r => r.UserId == refreshToken.UserId && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
            
            foreach (var activeToken in activeTokens)
            {
                activeToken.RevokedAt = DateTime.UtcNow;
                _dbContext.RefreshTokens.Update(activeToken);
            }
            await _dbContext.SaveChangesAsync();

            throw new UnauthorizedAccessException("Invalid refresh token. Token replay detected; all tokens revoked.");
        }

        refreshToken.RevokedAt = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(refreshToken.User);
        var claims = await _userManager.GetClaimsAsync(refreshToken.User);
        var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

        foreach (var role in roles)
        {
            var roleEntity = await _roleManager.FindByNameAsync(role);
            if (roleEntity != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                permissions.AddRange(roleClaims.Where(rc => rc.Type == "Permission").Select(rc => rc.Value));
            }
        }
        
        permissions = permissions.Distinct().ToList();

        var newJwtToken = _jwtTokenGenerator.GenerateAccessToken(refreshToken.User, roles, permissions);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken(refreshToken.UserId);

        refreshToken.ReplacedByToken = newRefreshToken.Token;

        _dbContext.RefreshTokens.Add(newRefreshToken);
        _dbContext.RefreshTokens.Update(refreshToken);
        await _dbContext.SaveChangesAsync();

        return new AuthenticationResponse
        {
            AccessToken = newJwtToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = newRefreshToken.ExpiresAt
        };
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _dbContext.RefreshTokens.SingleOrDefaultAsync(r => r.Token == token);
        if (refreshToken == null || !refreshToken.IsActive) return false;

        refreshToken.RevokedAt = DateTime.UtcNow;
        _dbContext.RefreshTokens.Update(refreshToken);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return string.Empty;

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return string.Empty;

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<bool> EnableTwoFactorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
        return result.Succeeded;
    }

    public async Task<bool> DisableTwoFactorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        return result.Succeeded;
    }

    public async Task<bool> LogoutAsync(string userId)
    {
        // Currently we only have refresh tokens to revoke, access tokens are stateless
        var userGuid = Guid.Parse(userId);
        var tokens = await _dbContext.RefreshTokens
            .Where(r => r.UserId == userGuid && r.IsActive)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.RevokedAt = DateTime.UtcNow;
            _dbContext.RefreshTokens.Update(t);
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }
}
