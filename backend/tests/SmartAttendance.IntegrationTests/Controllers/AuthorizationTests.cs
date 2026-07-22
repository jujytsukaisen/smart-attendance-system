using System.Net;
using FluentAssertions;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class AuthorizationTests : BaseIntegrationTest
{
    public AuthorizationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCompanies_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/v1/companies");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
