using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class MiddlewareTests : BaseIntegrationTest
{
    public MiddlewareTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_NonExistentRoute_Returns404()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/does-not-exist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_WithoutAuth_Returns401()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/companies", new { Name = "Test" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
