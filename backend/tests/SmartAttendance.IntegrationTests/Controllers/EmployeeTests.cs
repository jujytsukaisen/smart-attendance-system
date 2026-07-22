using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartAttendance.Application.Features.Employees.Commands;
using SmartAttendance.Application.Features.Employees.Queries;
using SmartAttendance.Application.Features.Employees.DTOs;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class EmployeeTests : BaseIntegrationTest
{
    public EmployeeTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateEmployee_WithValidData_ReturnsCreatedId()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        var companyCommand = new SmartAttendance.Application.Features.Companies.Commands.CreateCompanyCommand
        {
            Name = "Integration Test Company for Emp",
            LegalName = "Integration Test Company for Emp LLC",
            RegistrationNumber = "REG-EMP-999",
            TaxNumber = "TAX-EMP-999",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567890",
            Email = "contact-emp@integration.com",
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
            Name = "Integration Test Branch for Emp",
            Code = "IT-BR-EMP",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567891",
            Email = "branch-emp@integration.com",
            Latitude = 40.7128,
            Longitude = -74.0060,
            AllowedRadiusMeters = 100,
            TimeZone = "America/New_York",
            IsHeadOffice = true
        };
        var branchResponse = await client.PostAsJsonAsync("/api/v1/branches", branchCommand);
        branchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var branchId = await branchResponse.ReadWrappedResponseAsync<Guid>();

        var deptCommand = new SmartAttendance.Application.Features.Departments.Commands.CreateDepartmentCommand
        {
            CompanyId = companyId,
            BranchId = branchId,
            Name = "Integration Test Department for Emp",
            Code = "IT-DEPT-EMP",
            Description = "A department for integration tests"
        };
        var deptResponse = await client.PostAsJsonAsync("/api/v1/departments", deptCommand);
        deptResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var deptId = await deptResponse.ReadWrappedResponseAsync<Guid>();

        var policyId = Guid.NewGuid();
        await ExecuteDbContextAsync(async context =>
        {
            var policy = SmartAttendance.Domain.Entities.AttendancePolicy.Create(
                companyId,
                "Standard Policy",
                "ST-POL-01",
                "Standard Policy for Integration Tests",
                SmartAttendance.Domain.Enums.AttendanceMode.Office,
                true, // requireFaceRecognition
                false, // requireGPS
                false, // requireApprovedDevice
                false, // requireCompanyNetwork
                true, // allowMobileCheckIn
                true, // allowDesktopCheckIn
                true, // allowWebCheckIn
                false, // allowOfflineAttendance
                100, // allowedRadiusMeters
                false, // allowMultipleCheckIn
                false, // allowManualAttendance
                false, // requireManagerApproval
                1000 // maximumDistanceMeters
            );
            
            context.AttendancePolicies.Add(policy);
            await context.SaveChangesAsync();
            policyId = policy.Id;
        });

        var command = new CreateEmployeeCommand
        {
            CompanyId = companyId,
            BranchId = branchId,
            DepartmentId = deptId,
            AttendancePolicyId = policyId,
            EmployeeNumber = "EMP-001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@integration.com",
            PhoneNumber = "+19876543210",
            Street = "456 Emp St",
            City = "Empville",
            State = "ES",
            ZipCode = "54321",
            Country = "Empland",
            DateOfBirth = new DateTime(1990, 1, 1),
            HireDate = new DateTime(2023, 1, 1),
            JobTitle = "Software Engineer",
            Gender = SmartAttendance.Domain.Enums.Gender.Male
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/employees", command);

        // Assert
        var errorResponse = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"because the error was {errorResponse}");
        
        var employeeId = await response.ReadWrappedResponseAsync<Guid>();
        employeeId.Should().NotBeEmpty();

        // Verify retrieval
        var getResponse = await client.GetAsync($"/api/v1/employees/{employeeId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var employee = await getResponse.ReadWrappedResponseAsync<EmployeeDetailsDto>();
        employee.Should().NotBeNull();
        employee!.FirstName.Should().Be("John");
        employee.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetEmployees_ReturnsPaginatedList()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        // Act
        var response = await client.GetAsync("/api/v1/employees?pageNumber=1&pageSize=10");

        // Assert
        var errorResponse = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"because the error was {errorResponse}");
    }
}
