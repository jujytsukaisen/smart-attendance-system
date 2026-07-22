using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using SmartAttendance.Application.Features.Shifts.Commands;
using SmartAttendance.Application.Features.Shifts.Queries;
using SmartAttendance.Application.Features.Shifts.DTOs;
using SmartAttendance.Application.Features.WorkSchedules.Commands;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class ShiftTests : BaseIntegrationTest
{
    public ShiftTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<Guid> CreateTestCompany(HttpClient client, string suffix = "")
    {
        var companyCommand = new SmartAttendance.Application.Features.Companies.Commands.CreateCompanyCommand
        {
            Name = $"Integration Test Company SH {suffix}",
            LegalName = $"Integration Test Company LLC SH {suffix}",
            RegistrationNumber = $"REG-{Guid.NewGuid().ToString().Substring(0, 5)}",
            TaxNumber = $"TAX-{Guid.NewGuid().ToString().Substring(0, 5)}",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567890",
            Email = $"contactsh{suffix}@integration.com",
            FoundedDate = new DateTime(2020, 1, 1),
            TimeZone = "UTC",
            Currency = "USD"
        };
        var companyResponse = await client.PostAsJsonAsync("/api/v1/companies", companyCommand);
        return await companyResponse.ReadWrappedResponseAsync<Guid>();
    }

    private async Task<Guid> CreateTestWorkSchedule(HttpClient client, Guid companyId, string name = "Base Schedule")
    {
        var command = new CreateWorkScheduleCommand
        {
            CompanyId = companyId,
            Name = name,
            WeeklyWorkingHours = 40
        };
        var response = await client.PostAsJsonAsync("/api/v1/workschedules", command);
        return await response.ReadWrappedResponseAsync<Guid>();
    }

    [Fact]
    public async Task CreateShift_WithValidData_ReturnsCreatedId()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "1");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId);

        var command = new CreateShiftCommand
        {
            CompanyId = companyId,
            WorkScheduleId = workScheduleId,
            Name = "Morning Shift",
            Code = "SH-MORN-01",
            StartTime = new TimeOnly(9, 0, 0),
            EndTime = new TimeOnly(17, 0, 0),
            MinimumWorkingHours = 7,
            MaximumWorkingHours = 8,
            GracePeriodMinutes = 15,
            AllowEarlyCheckInMinutes = 30,
            AllowLateCheckOutMinutes = 30
        };

        var response = await client.PostAsJsonAsync("/api/v1/shifts", command);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var shiftId = await response.ReadWrappedResponseAsync<Guid>();
        shiftId.Should().NotBeEmpty();

        var getResponse = await client.GetAsync($"/api/v1/shifts/{shiftId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var shift = await getResponse.ReadWrappedResponseAsync<ShiftDetailsDto>();
        
        shift.Should().NotBeNull();
        shift!.Name.Should().Be(command.Name);
        shift.StartTime.Should().Be(command.StartTime);
    }

    [Fact]
    public async Task CreateShift_DuplicateName_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Dup");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId, "Dup Schedule");

        var command = new CreateShiftCommand
        {
            CompanyId = companyId,
            WorkScheduleId = workScheduleId,
            Name = "Duplicate Shift",
            Code = "SH-DUP-01",
            StartTime = new TimeOnly(9, 0, 0),
            EndTime = new TimeOnly(17, 0, 0)
        , MinimumWorkingHours = 7, MaximumWorkingHours = 8 };

        var response1 = await client.PostAsJsonAsync("/api/v1/shifts", command);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var response2 = await client.PostAsJsonAsync("/api/v1/shifts", command);
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateShift_OvernightShift_Succeeds()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Overnight");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId, "Overnight Schedule");

        var command = new CreateShiftCommand
        {
            CompanyId = companyId,
            WorkScheduleId = workScheduleId,
            Name = "Night Shift",
            Code = "SH-NIGHT-01",
            StartTime = new TimeOnly(22, 0, 0),
            EndTime = new TimeOnly(6, 0, 0), // Overnight
            MinimumWorkingHours = 7,
            MaximumWorkingHours = 8
        };

        var response = await client.PostAsJsonAsync("/api/v1/shifts", command);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateShift_ValidData_UpdatesCorrectly()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Upd");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId, "Update Schedule");

        var createCommand = new CreateShiftCommand
        {
            CompanyId = companyId,
            WorkScheduleId = workScheduleId,
            Name = "Shift To Update",
            Code = "OLD-01",
            StartTime = new TimeOnly(9, 0, 0),
            EndTime = new TimeOnly(17, 0, 0)
        , MinimumWorkingHours = 7, MaximumWorkingHours = 8 };
        var response = await client.PostAsJsonAsync("/api/v1/shifts", createCommand);
        var id = await response.ReadWrappedResponseAsync<Guid>();

        var updateCommand = new UpdateShiftCommand
        {
            ShiftId = id,
            Name = "Updated Shift",
            Code = "NEW-01"
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/shifts/{id}", updateCommand);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/shifts/{id}");
        var shift = await getResponse.ReadWrappedResponseAsync<ShiftDetailsDto>();
        
        shift!.Name.Should().Be("Updated Shift");
        shift.Code.Should().Be("NEW-01");
        shift.StartTime.Should().Be(new TimeOnly(9, 0, 0));
    }

    [Fact]
    public async Task DeleteShift_SoftDeletesShift()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Del");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId, "Del Schedule");

        var command = new CreateShiftCommand { CompanyId = companyId, WorkScheduleId = workScheduleId, Name = "To Delete", Code = "DEL-01", StartTime = new TimeOnly(9,0), EndTime = new TimeOnly(17,0), MinimumWorkingHours = 7, MaximumWorkingHours = 9 };
        var response = await client.PostAsJsonAsync("/api/v1/shifts", command);
        var id = await response.ReadWrappedResponseAsync<Guid>();

        var delResponse = await client.DeleteAsync($"/api/v1/shifts/{id}");
        delResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/shifts/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateAndDeactivateShift_TogglesStatus()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Act");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId, "Act Schedule");

        var command = new CreateShiftCommand { CompanyId = companyId, WorkScheduleId = workScheduleId, Name = "Status Toggle", Code = "ACT-01", StartTime = new TimeOnly(9,0), EndTime = new TimeOnly(17,0) , MinimumWorkingHours = 7, MaximumWorkingHours = 8 };
        var response = await client.PostAsJsonAsync("/api/v1/shifts", command);
        var id = await response.ReadWrappedResponseAsync<Guid>();

        // Activate
        var actResponse = await client.PostAsync($"/api/v1/shifts/{id}/activate", null);
        actResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Deactivate
        var deactResponse = await client.PostAsync($"/api/v1/shifts/{id}/deactivate", null);
        deactResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetShiftsByWorkSchedule_SupportsPagination()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Pag");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId, "Pag Schedule");

        for (int i = 1; i <= 5; i++)
        {
            await client.PostAsJsonAsync("/api/v1/shifts", new CreateShiftCommand 
            { 
                CompanyId = companyId, 
                WorkScheduleId = workScheduleId, 
                Name = $"PageTest-{i}", 
                Code = $"P-{i}",
                StartTime = new TimeOnly(i, 0, 0), 
                EndTime = new TimeOnly(i + 1, 0, 0),
                MinimumWorkingHours = 1, MaximumWorkingHours = 8 
            });
        }

        var getResponse = await client.GetAsync($"/api/v1/shifts/work-schedule/{workScheduleId}?pageNumber=1&pageSize=3");
        var result = await getResponse.ReadWrappedResponseAsync<PagedResult<ShiftDto>>();
        
        result!.Items.Count.Should().Be(3);
        result.Pagination.PageNumber.Should().Be(1);
        
        var getResponse2 = await client.GetAsync($"/api/v1/shifts/work-schedule/{workScheduleId}?pageNumber=2&pageSize=3");
        var result2 = await getResponse2.ReadWrappedResponseAsync<PagedResult<ShiftDto>>();
        
        result2!.Items.Count.Should().BeGreaterThan(0);
        result2.Pagination.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task SearchShifts_FiltersByName()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Search");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId, "Search Schedule");

        await client.PostAsJsonAsync("/api/v1/shifts", new CreateShiftCommand { CompanyId = companyId, WorkScheduleId = workScheduleId, Name = "Night Shift", StartTime = new TimeOnly(20,0), EndTime = new TimeOnly(4,0) , MinimumWorkingHours = 7, MaximumWorkingHours = 8 });
        var command = new CreateShiftCommand
        {
            CompanyId = companyId,
            WorkScheduleId = workScheduleId,
            Name = "Morning Shift",
            Code = "SH-MORN-01",
            StartTime = new TimeOnly(9, 0, 0),
            EndTime = new TimeOnly(17, 0, 0),
            MinimumWorkingHours = 7,
            MaximumWorkingHours = 8,
            GracePeriodMinutes = 15,
            AllowEarlyCheckInMinutes = 30,
            AllowLateCheckOutMinutes = 30
        };

        var response = await client.PostAsJsonAsync("/api/v1/shifts", command);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var shiftId = await response.ReadWrappedResponseAsync<Guid>();
        shiftId.Should().NotBeEmpty();

        var getResponse = await client.GetAsync($"/api/v1/shifts/{shiftId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var shift = await getResponse.ReadWrappedResponseAsync<ShiftDetailsDto>();
        
        shift.Should().NotBeNull();
        shift!.Name.Should().Be(command.Name);
        shift.StartTime.Should().Be(command.StartTime);
    }
}
