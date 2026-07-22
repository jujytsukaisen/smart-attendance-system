using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartAttendance.Application.Features.Departments.Commands;
using SmartAttendance.Application.Features.Departments.Queries;
using SmartAttendance.Application.Features.Departments.DTOs;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class DepartmentTests : BaseIntegrationTest
{
    public DepartmentTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_WithValidData_ReturnsCreatedId()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        var companyCommand = new SmartAttendance.Application.Features.Companies.Commands.CreateCompanyCommand
        {
            Name = "Integration Test Company for Dept",
            LegalName = "Integration Test Company for Dept LLC",
            RegistrationNumber = "REG-DP-999",
            TaxNumber = "TAX-DP-999",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567890",
            Email = "contact-dp@integration.com",
            FoundedDate = new DateTime(2020, 1, 1),
            TimeZone = "UTC",
            Currency = "USD"
        };
        var companyResponse = await client.PostAsJsonAsync("/api/v1/companies", companyCommand);
        companyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var companyId = await companyResponse.ReadWrappedResponseAsync<Guid>();

        var branchCommand = new SmartAttendance.Application.Features.Branches.Commands.CreateBranchCommand
        {
            CompanyId = companyId,
            Name = "Integration Test Branch",
            Code = "IT-BR",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567891",
            Email = "branch-dp@integration.com",
            Latitude = 40.7128,
            Longitude = -74.0060,
            AllowedRadiusMeters = 100,
            TimeZone = "America/New_York",
            IsHeadOffice = true
        };
        var branchResponse = await client.PostAsJsonAsync("/api/v1/branches", branchCommand);
        branchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var branchId = await branchResponse.ReadWrappedResponseAsync<Guid>();

        var command = new CreateDepartmentCommand
        {
            CompanyId = companyId,
            BranchId = branchId,
            Name = "Integration Test Department",
            Code = "IT-DEPT",
            Description = "A department for integration tests"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/departments", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var departmentId = await response.ReadWrappedResponseAsync<Guid>();
        departmentId.Should().NotBeEmpty();

        // Verify retrieval
        var getResponse = await client.GetAsync($"/api/v1/departments/{departmentId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var department = await getResponse.ReadWrappedResponseAsync<DepartmentDetailsDto>();
        department.Should().NotBeNull();
        department!.Name.Should().Be("Integration Test Department");
    }

    [Fact]
    public async Task GetDepartments_ReturnsPaginatedList()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        // Act
        var response = await client.GetAsync("/api/v1/departments?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
