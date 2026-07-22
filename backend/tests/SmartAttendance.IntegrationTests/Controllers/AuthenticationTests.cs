using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartAttendance.Application.Features.Authentication.Commands;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class AuthenticationTests : BaseIntegrationTest
{
    public AuthenticationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsJwtToken()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "admin@smartattendance.local",
            Password = "Admin@123456"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.ReadWrappedResponseAsync<AuthResponseDto>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var command = new
        {
            Email = "wrong@smartattendance.local",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool RequiresTwoFactor { get; set; }
}
