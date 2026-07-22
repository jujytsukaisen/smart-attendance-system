using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartAttendance.Application.Features.Companies.Commands;
using SmartAttendance.Application.Features.Companies.DTOs;
using SmartAttendance.Application.Features.Companies.Queries;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class CompanyTests : BaseIntegrationTest
{
    public CompanyTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateCompany_WithValidData_ReturnsCreatedId()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        var command = new CreateCompanyCommand
        {
            Name = "Integration Test Company",
            LegalName = "Integration Test Company LLC",
            RegistrationNumber = "REG-999",
            TaxNumber = "TAX-999",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567890",
            Email = "contact@integration.com",
            FoundedDate = new DateTime(2020, 1, 1),
            TimeZone = "UTC",
            Currency = "USD"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/companies", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var companyId = await response.ReadWrappedResponseAsync<Guid>();
        companyId.Should().NotBeEmpty();

        // Verify retrieval
        var getResponse = await client.GetAsync($"/api/v1/companies/{companyId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var company = await getResponse.ReadWrappedResponseAsync<CompanyDetailsDto>();
        company.Should().NotBeNull();
        company!.Name.Should().Be("Integration Test Company");
    }

    [Fact]
    public async Task GetCompanies_ReturnsPaginatedList()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        // Act
        var response = await client.GetAsync("/api/v1/companies?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
