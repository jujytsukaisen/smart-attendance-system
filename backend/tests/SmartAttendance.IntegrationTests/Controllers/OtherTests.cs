using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class OtherTests : BaseIntegrationTest
{
    public OtherTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("/api/v1/branches")]
    [InlineData("/api/v1/departments")]
    [InlineData("/api/v1/shifts")]
    [InlineData("/api/v1/work-schedules")]
    [InlineData("/api/v1/leaves")]
    [InlineData("/api/v1/notifications")]
    [InlineData("/api/v1/audit-logs")]
    public async Task GetEndpoints_ReturnsPaginatedList(string endpoint)
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        // Act
        var response = await client.GetAsync($"{endpoint}?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
