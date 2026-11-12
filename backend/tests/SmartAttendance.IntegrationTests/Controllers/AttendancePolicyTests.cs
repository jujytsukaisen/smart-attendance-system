using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SmartAttendance.Application.Features.AttendancePolicies.Commands;
using SmartAttendance.Application.Features.AttendancePolicies.Queries;
using SmartAttendance.Application.Features.AttendancePolicies.DTOs;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Enums;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class AttendancePolicyTests : BaseIntegrationTest
{
    public AttendancePolicyTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<Guid> CreateTestCompany(HttpClient client, string suffix = "")
    {
        var companyCommand = new SmartAttendance.Application.Features.Companies.Commands.CreateCompanyCommand
        {
            Name = $"Integration Test Company {suffix}",
            LegalName = $"Integration Test Company LLC {suffix}",
            RegistrationNumber = $"REG-{Guid.NewGuid().ToString().Substring(0, 5)}",
            TaxNumber = $"TAX-{Guid.NewGuid().ToString().Substring(0, 5)}",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567890",
            Email = $"contact{suffix}@integration.com",
            FoundedDate = new DateTime(2020, 1, 1),
            TimeZone = "UTC",
            Currency = "USD"
        };
        var companyResponse = await client.PostAsJsonAsync("/api/v1/companies", companyCommand);
        return await companyResponse.ReadWrappedResponseAsync<Guid>();
    }

    [Fact]
    public async Task CreateAttendancePolicy_WithValidData_ReturnsCreatedId()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "1");

        var command = new CreateAttendancePolicyCommand
        {
            CompanyId = companyId,
            Name = "Standard Office Policy",
            Code = "STD-OFF-01",
            Description = "Standard office working policy",
            AttendanceMode = AttendanceMode.Office,
            RequireFaceRecognition = true,
            RequireCompanyNetwork = true,
            AllowDesktopCheckIn = true,
            AllowWebCheckIn = true,
            AllowedRadiusMeters = 50,
            MaximumDistanceMeters = 100
        };

        var response = await client.PostAsJsonAsync("/api/v1/attendancepolicies", command);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var policyId = await response.ReadWrappedResponseAsync<Guid>();
        policyId.Should().NotBeEmpty();

        var getResponse = await client.GetAsync($"/api/v1/attendancepolicies/{policyId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var policy = await getResponse.ReadWrappedResponseAsync<AttendancePolicyDetailsDto>();
        
        policy!.Name.Should().Be(command.Name);
        policy!.Code.Should().Be(command.Code);
        policy!.AttendanceMode.Should().Be(AttendanceMode.Office);
        policy!.IsActive.Should().BeFalse(); // By default newly created policy is inactive
    }
    
    [Fact]
    public async Task CreateAttendancePolicy_WithInvalidModeCombination_ReturnsUnprocessableEntity()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        var command = new CreateAttendancePolicyCommand
        {
            CompanyId = Guid.NewGuid(),
            Name = "Invalid Office Policy",
            Code = "INV-OFF-01",
            AttendanceMode = AttendanceMode.Office,
            RequireFaceRecognition = false, // Invalid: Office mode requires Face Recognition
            AllowedRadiusMeters = 50
        };

        var response = await client.PostAsJsonAsync("/api/v1/attendancepolicies", command);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity); // Due to Validation problem
    }

    [Fact]
    public async Task CreateAttendancePolicy_DuplicateCode_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "2");

        var command = new CreateAttendancePolicyCommand
        {
            CompanyId = companyId,
            Name = "Policy One",
            Code = "DUP-01",
            AttendanceMode = AttendanceMode.Remote,
        };

        await client.PostAsJsonAsync("/api/v1/attendancepolicies", command);
        
        // Change name, keep same code
        command.Name = "Policy Two";
        var response = await client.PostAsJsonAsync("/api/v1/attendancepolicies", command);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateAttendancePolicy_WithValidData_ReturnsSuccess()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "3");

        var createCommand = new CreateAttendancePolicyCommand
        {
            CompanyId = companyId,
            Name = "Initial Policy",
            Code = "INIT-01",
            AttendanceMode = AttendanceMode.Remote,
        };

        var createResponse = await client.PostAsJsonAsync("/api/v1/attendancepolicies", createCommand);
        var policyId = await createResponse.ReadWrappedResponseAsync<Guid>();

        var updateCommand = new UpdateAttendancePolicyCommand
        {
            Id = policyId,
            CompanyId = companyId,
            Name = "Updated Policy",
            Code = "INIT-01", // keeping same code
            AttendanceMode = AttendanceMode.Office,
            RequireFaceRecognition = true,
            AllowedRadiusMeters = 100,
            MaximumDistanceMeters = 200
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/attendancepolicies/{policyId}", updateCommand);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await client.GetAsync($"/api/v1/attendancepolicies/{policyId}");
        var policy = await getResponse.ReadWrappedResponseAsync<AttendancePolicyDetailsDto>();
        policy!.Name.Should().Be("Updated Policy");
        policy!.AttendanceMode.Should().Be(AttendanceMode.Office);
    }

    [Fact]
    public async Task Activate_And_Deactivate_AttendancePolicy_ChangesStatus()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "4");

        var createCommand = new CreateAttendancePolicyCommand
        {
            CompanyId = companyId,
            Name = "Activation Policy",
            Code = "ACT-01",
            AttendanceMode = AttendanceMode.Remote,
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/attendancepolicies", createCommand);
        var policyId = await createResponse.ReadWrappedResponseAsync<Guid>();

        // Activate
        var activateResponse = await client.PostAsync($"/api/v1/attendancepolicies/{policyId}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var policyActive = await (await client.GetAsync($"/api/v1/attendancepolicies/{policyId}")).ReadWrappedResponseAsync<AttendancePolicyDetailsDto>();
        policyActive.Should().NotBeNull();
        policyActive!.IsActive.Should().BeTrue();

        // Deactivate
        var deactivateResponse = await client.PostAsync($"/api/v1/attendancepolicies/{policyId}/deactivate", null);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var policyDeactive = await (await client.GetAsync($"/api/v1/attendancepolicies/{policyId}")).ReadWrappedResponseAsync<AttendancePolicyDetailsDto>();
        policyDeactive.Should().NotBeNull();
        policyDeactive!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAttendancePolicy_SoftDeletes_And_ReturnsNotFound()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "5");

        var createCommand = new CreateAttendancePolicyCommand
        {
            CompanyId = companyId,
            Name = "Delete Policy",
            Code = "DEL-01",
            AttendanceMode = AttendanceMode.Remote,
        };
        var createResponse = await client.PostAsJsonAsync("/api/v1/attendancepolicies", createCommand);
        var policyId = await createResponse.ReadWrappedResponseAsync<Guid>();

        var deleteResponse = await client.DeleteAsync($"/api/v1/attendancepolicies/{policyId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Attempting to get it should now return 404
        var getResponse = await client.GetAsync($"/api/v1/attendancepolicies/{policyId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Restore
        var restoreResponse = await client.PostAsync($"/api/v1/attendancepolicies/{policyId}/restore", null);
        restoreResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify restored
        var getRestoredResponse = await client.GetAsync($"/api/v1/attendancepolicies/{policyId}");
        getRestoredResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchAndPagination_ReturnsCorrectResults()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "6");

        // Create 3 policies
        for (int i = 1; i <= 3; i++)
        {
            await client.PostAsJsonAsync("/api/v1/attendancepolicies", new CreateAttendancePolicyCommand
            {
                CompanyId = companyId,
                Name = $"Searchable Policy {i}",
                Code = $"SRCH-0{i}",
                AttendanceMode = AttendanceMode.Remote,
            });
        }

        var response = await client.GetAsync("/api/v1/attendancepolicies?SearchTerm=Searchable&PageNumber=1&PageSize=2");
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error: {err}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadWrappedResponseAsync<PagedResult<AttendancePolicyListDto>>();

        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(2);
        result.Pagination.TotalCount.Should().BeGreaterOrEqualTo(3);
        result.Pagination.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetActivePolicies_ReturnsOnlyActive()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "7");

        var policy1Id = await (await client.PostAsJsonAsync("/api/v1/attendancepolicies", new CreateAttendancePolicyCommand { CompanyId = companyId, Name = "P1", Code = "P1", AttendanceMode = AttendanceMode.Remote })).ReadWrappedResponseAsync<Guid>();
        var policy2Id = await (await client.PostAsJsonAsync("/api/v1/attendancepolicies", new CreateAttendancePolicyCommand { CompanyId = companyId, Name = "P2", Code = "P2", AttendanceMode = AttendanceMode.Remote })).ReadWrappedResponseAsync<Guid>();

        await client.PostAsync($"/api/v1/attendancepolicies/{policy1Id}/activate", null);
        // policy2 remains inactive

        var response = await client.GetAsync("/api/v1/attendancepolicies?IsActive=true&PageNumber=1&PageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadWrappedResponseAsync<PagedResult<AttendancePolicyListDto>>();

        result.Should().NotBeNull();
        result!.Items.Should().Contain(p => p.Id == policy1Id);
        result.Items.Should().NotContain(p => p.Id == policy2Id);
    }
}
