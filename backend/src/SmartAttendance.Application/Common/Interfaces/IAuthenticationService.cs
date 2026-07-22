using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationResponse> LoginAsync(string email, string password);
    Task<string> RegisterAsync(string email, string password, string firstName, string lastName);
    Task<AuthenticationResponse> RefreshTokenAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<string> GenerateEmailConfirmationTokenAsync(string email);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<bool> EnableTwoFactorAsync(string userId);
    Task<bool> DisableTwoFactorAsync(string userId);
    Task<bool> LogoutAsync(string userId);
}
