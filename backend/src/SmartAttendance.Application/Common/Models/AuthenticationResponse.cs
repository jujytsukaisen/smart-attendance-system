using System;

namespace SmartAttendance.Application.Common.Models;

public class AuthenticationResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
