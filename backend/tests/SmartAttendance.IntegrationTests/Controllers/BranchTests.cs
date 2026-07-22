using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartAttendance.Application.Features.Branches.Commands;
using SmartAttendance.Application.Features.Branches.Queries;
using SmartAttendance.Application.Features.Branches.DTOs;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class BranchTests : BaseIntegrationTest
{
    public BranchTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateBranch_WithValidData_ReturnsCreatedId()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        var companyCommand = new SmartAttendance.Application.Features.Companies.Commands.CreateCompanyCommand
        {
            Name = "Integration Test Company for Branch",
            LegalName = "Integration Test Company for Branch LLC",
            RegistrationNumber = "REG-BR-999",
            TaxNumber = "TAX-BR-999",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567890",
            Email = "contact-br@integration.com",
            FoundedDate = new DateTime(2020, 1, 1),
            TimeZone = "UTC",
            Currency = "USD"
        };
        var companyResponse = await client.PostAsJsonAsync("/api/v1/companies", companyCommand);
        companyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var companyId = await companyResponse.ReadWrappedResponseAsync<Guid>();

        var command = new CreateBranchCommand
        {
            CompanyId = companyId,
            Name = "Integration Test Branch",
            Code = "IT-BR",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567891",
            Email = "branch@integration.com",
            Latitude = 40.7128,
            Longitude = -74.0060,
            AllowedRadiusMeters = 100,
            TimeZone = "America/New_York",
            IsHeadOffice = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/branches", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var branchId = await response.ReadWrappedResponseAsync<Guid>();
        branchId.Should().NotBeEmpty();

        // Verify retrieval
        var getResponse = await client.GetAsync($"/api/v1/branches/{branchId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var branch = await getResponse.ReadWrappedResponseAsync<BranchDetailsDto>();
        branch.Should().NotBeNull();
        branch!.Name.Should().Be("Integration Test Branch");
    }

    [Fact]
    public async Task GetBranches_ReturnsPaginatedList()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        // Act
        var response = await client.GetAsync("/api/v1/branches?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
