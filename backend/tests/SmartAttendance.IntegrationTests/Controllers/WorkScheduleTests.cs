using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using SmartAttendance.Application.Features.Shifts.Commands;
using SmartAttendance.Application.Features.WorkSchedules.Commands;
using SmartAttendance.Application.Features.WorkSchedules.Queries;
using SmartAttendance.Application.Features.WorkSchedules.DTOs;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class WorkScheduleTests : BaseIntegrationTest
{
    public WorkScheduleTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<Guid> CreateTestCompany(HttpClient client, string suffix = "")
    {
        var companyCommand = new SmartAttendance.Application.Features.Companies.Commands.CreateCompanyCommand
        {
            Name = $"Integration Test Company WS {suffix}",
            LegalName = $"Integration Test Company LLC WS {suffix}",
            RegistrationNumber = $"REG-{Guid.NewGuid().ToString().Substring(0, 5)}",
            TaxNumber = $"TAX-{Guid.NewGuid().ToString().Substring(0, 5)}",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567890",
            Email = $"contactws{suffix}@integration.com",
            FoundedDate = new DateTime(2020, 1, 1),
            TimeZone = "UTC",
            Currency = "USD"
        };
        var companyResponse = await client.PostAsJsonAsync("/api/v1/companies", companyCommand);
        return await companyResponse.ReadWrappedResponseAsync<Guid>();
    }

    [Fact]
    public async Task CreateWorkSchedule_WithValidData_ReturnsCreatedId()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "1");

        var command = new CreateWorkScheduleCommand
        {
            CompanyId = companyId,
            Name = "Standard Morning Schedule",
            Description = "Standard 40 hours",
            IsFlexible = false,
            WeeklyWorkingHours = 40,
            GracePeriodMinutes = 15,
            AllowEarlyCheckInMinutes = 30,
            AllowLateCheckOutMinutes = 30
        };

        var response = await client.PostAsJsonAsync("/api/v1/workschedules", command);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var scheduleId = await response.ReadWrappedResponseAsync<Guid>();
        scheduleId.Should().NotBeEmpty();

        var getResponse = await client.GetAsync($"/api/v1/workschedules/{scheduleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var schedule = await getResponse.ReadWrappedResponseAsync<WorkScheduleDetailsDto>();
        
        schedule.Should().NotBeNull();
        schedule!.Name.Should().Be(command.Name);
        schedule.IsActive.Should().BeFalse();
    }
    
    [Fact]
    public async Task CreateWorkSchedule_DuplicateName_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Dup");

        var command = new CreateWorkScheduleCommand
        {
            CompanyId = companyId,
            Name = "Duplicate Schedule",
            WeeklyWorkingHours = 40
        };

        var response1 = await client.PostAsJsonAsync("/api/v1/workschedules", command);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        var response2 = await client.PostAsJsonAsync("/api/v1/workschedules", command);
        response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateWorkSchedule_ValidData_UpdatesCorrectly()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Upd");

        var createCommand = new CreateWorkScheduleCommand
        {
            CompanyId = companyId,
            Name = "Schedule To Update",
            WeeklyWorkingHours = 40
        };
        var response = await client.PostAsJsonAsync("/api/v1/workschedules", createCommand);
        var id = await response.ReadWrappedResponseAsync<Guid>();

        var updateCommand = new UpdateWorkScheduleCommand
        {
            WorkScheduleId = id,
            Name = "Updated Schedule",
            Description = "Updated description",
            IsFlexible = true,
            WeeklyWorkingHours = 35,
            GracePeriodMinutes = 10,
            AllowEarlyCheckInMinutes = 15,
            AllowLateCheckOutMinutes = 15
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/workschedules/{id}", updateCommand);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/workschedules/{id}");
        var schedule = await getResponse.ReadWrappedResponseAsync<WorkScheduleDetailsDto>();
        schedule!.Name.Should().Be("Updated Schedule");
    }

    [Fact]
    public async Task DeleteWorkSchedule_SoftDeletesSchedule()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Del");

        var command = new CreateWorkScheduleCommand { CompanyId = companyId, Name = "To Delete", WeeklyWorkingHours = 40 };
        var response = await client.PostAsJsonAsync("/api/v1/workschedules", command);
        var id = await response.ReadWrappedResponseAsync<Guid>();

        var delResponse = await client.DeleteAsync($"/api/v1/workschedules/{id}");
        delResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/workschedules/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateAndDeactivateWorkSchedule_TogglesStatus()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Act");

        var command = new CreateWorkScheduleCommand { CompanyId = companyId, Name = "Status Toggle", WeeklyWorkingHours = 40 };
        var response = await client.PostAsJsonAsync("/api/v1/workschedules", command);
        var id = await response.ReadWrappedResponseAsync<Guid>();

        var shiftCommand = new CreateShiftCommand { CompanyId = companyId, WorkScheduleId = id, Name = "Shift", Code = "SH", StartTime = new TimeOnly(9,0), EndTime = new TimeOnly(17,0), MinimumWorkingHours = 7, MaximumWorkingHours = 8 };
        await client.PostAsJsonAsync("/api/v1/shifts", shiftCommand);

        // Activate
        var actResponse = await client.PostAsync($"/api/v1/workschedules/{id}/activate", null);
        actResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/workschedules/{id}");
        var schedule = await getResponse.ReadWrappedResponseAsync<WorkScheduleDetailsDto>();
        schedule!.IsActive.Should().BeTrue();

        // Deactivate
        var deactResponse = await client.PostAsync($"/api/v1/workschedules/{id}/deactivate", null);
        deactResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        getResponse = await client.GetAsync($"/api/v1/workschedules/{id}");
        schedule = await getResponse.ReadWrappedResponseAsync<WorkScheduleDetailsDto>();
        schedule!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetWorkSchedules_SupportsPagination()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Pag");

        for (int i = 1; i <= 5; i++)
        {
            await client.PostAsJsonAsync("/api/v1/workschedules", new CreateWorkScheduleCommand { CompanyId = companyId, Name = $"PageTest-{i}", WeeklyWorkingHours = 40 });
        }

        var getResponse = await client.GetAsync($"/api/v1/workschedules?pageNumber=1&pageSize=3");
        var result = await getResponse.ReadWrappedResponseAsync<PagedResult<WorkScheduleListDto>>();
        
        result!.Items.Count.Should().Be(3);
        result.Pagination.PageNumber.Should().Be(1);
        
        var getResponse2 = await client.GetAsync($"/api/v1/workschedules?pageNumber=2&pageSize=3");
        var result2 = await getResponse2.ReadWrappedResponseAsync<PagedResult<WorkScheduleListDto>>();
        
        result2!.Items.Count.Should().BeGreaterThan(0);
        result2.Pagination.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task SearchWorkSchedules_FiltersByName()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Search");

        await client.PostAsJsonAsync("/api/v1/workschedules", new CreateWorkScheduleCommand { CompanyId = companyId, Name = "Night Shift Schedule", WeeklyWorkingHours = 40 });
        await client.PostAsJsonAsync("/api/v1/workschedules", new CreateWorkScheduleCommand { CompanyId = companyId, Name = "Morning Shift Schedule", WeeklyWorkingHours = 40 });

        var getResponse = await client.GetAsync($"/api/v1/workschedules?searchTerm=Night");
        var result = await getResponse.ReadWrappedResponseAsync<PagedResult<WorkScheduleListDto>>();
        
        result!.Items.Should().Contain(x => x.Name == "Night Shift Schedule");
        result.Items.Should().NotContain(x => x.Name == "Morning Shift Schedule");
    }
}
