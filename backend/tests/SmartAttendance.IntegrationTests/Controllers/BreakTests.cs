using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using SmartAttendance.Application.Features.Breaks.Commands;
using SmartAttendance.Application.Features.Breaks.Queries;
using SmartAttendance.Application.Features.Shifts.Commands;
using SmartAttendance.Application.Features.WorkSchedules.Commands;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Breaks.DTOs;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class BreakTests : BaseIntegrationTest
{
    public BreakTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<Guid> CreateTestCompany(HttpClient client, string suffix = "")
    {
        var companyCommand = new SmartAttendance.Application.Features.Companies.Commands.CreateCompanyCommand
        {
            Name = $"Integration Test Company BR {suffix}",
            LegalName = $"Integration Test Company LLC BR {suffix}",
            RegistrationNumber = $"REG-{Guid.NewGuid().ToString().Substring(0, 5)}",
            TaxNumber = $"TAX-{Guid.NewGuid().ToString().Substring(0, 5)}",
            AddressStreet = "123 Test St", AddressCity = "Testville", AddressState = "TS", AddressCountry = "Testland", AddressZipCode = "12345",
            PhoneNumber = "+1234567890",
            Email = $"contactbr{suffix}@integration.com",
            FoundedDate = new DateTime(2020, 1, 1),
            TimeZone = "UTC",
            Currency = "USD"
        };
        var companyResponse = await client.PostAsJsonAsync("/api/v1/companies", companyCommand);
        companyResponse.EnsureSuccessStatusCode();
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
        response.EnsureSuccessStatusCode();
        return await response.ReadWrappedResponseAsync<Guid>();
    }

    private async Task<Guid> CreateTestShift(HttpClient client, Guid companyId, Guid workScheduleId, string name = "Base Shift", int startHour = 9, int endHour = 17)
    {
        var command = new CreateShiftCommand
        {
            CompanyId = companyId,
            WorkScheduleId = workScheduleId,
            Name = name,
            Code = $"SH-{Guid.NewGuid().ToString().Substring(0, 5)}",
            StartTime = new TimeOnly(startHour, 0, 0),
            EndTime = new TimeOnly(endHour, 0, 0),
            MinimumWorkingHours = 7,
            MaximumWorkingHours = 8
        };
        var response = await client.PostAsJsonAsync("/api/v1/shifts", command);
        response.EnsureSuccessStatusCode();
        return await response.ReadWrappedResponseAsync<Guid>();
    }

    [Fact]
    public async Task CreateBreak_WithValidData_ReturnsCreatedId()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "1");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId);
        var shiftId = await CreateTestShift(client, companyId, workScheduleId);

        var command = new CreateBreakCommand
        {
            ShiftId = shiftId,
            CompanyId = companyId, Name = "Lunch Break", DurationMinutes = 60,
            StartTime = new TimeOnly(12, 0, 0),
            EndTime = new TimeOnly(13, 0, 0),
            IsPaid = false,
            IsMandatory = true
        };

        var response = await client.PostAsJsonAsync("/api/v1/breaks", command);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var breakId = await response.ReadWrappedResponseAsync<Guid>();
        breakId.Should().NotBeEmpty();

        var getResponse = await client.GetAsync($"/api/v1/breaks/{breakId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var @break = await getResponse.ReadWrappedResponseAsync<BreakDetailsDto>();
        
        @break.Should().NotBeNull();
        @break!.Name.Should().Be(command.Name);
        @break.StartTime.Should().Be(command.StartTime);
    }

    [Fact]
    public async Task CreateBreak_OutsideShiftBoundaries_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "2");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId);
        var shiftId = await CreateTestShift(client, companyId, workScheduleId, "9to17", 9, 17);

        var command = new CreateBreakCommand
        {
            ShiftId = shiftId,
            Name = "Dinner Break",
            StartTime = new TimeOnly(18, 0, 0), // Outside 9-17 shift
            EndTime = new TimeOnly(19, 0, 0),
            IsPaid = false,
            IsMandatory = true
        };

        var response = await client.PostAsJsonAsync("/api/v1/breaks", command);
        
        // This should fail via Domain invariant and return BadRequest/UnprocessableEntity or Conflict
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Match(s => s == HttpStatusCode.Conflict || s == HttpStatusCode.BadRequest || s == HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateBreak_OverlappingBreaks_ReturnsConflict()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "3");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId);
        var shiftId = await CreateTestShift(client, companyId, workScheduleId);

        var command1 = new CreateBreakCommand
        {
            ShiftId = shiftId,
            CompanyId = companyId, Name = "Lunch Break", DurationMinutes = 60,
            StartTime = new TimeOnly(12, 0, 0),
            EndTime = new TimeOnly(13, 0, 0)
        };
        await client.PostAsJsonAsync("/api/v1/breaks", command1);

        var command2 = new CreateBreakCommand
        {
            ShiftId = shiftId,
            Name = "Another Break",
            StartTime = new TimeOnly(12, 30, 0), // Overlaps with 12:00-13:00
            EndTime = new TimeOnly(13, 30, 0)
        };
        var response = await client.PostAsJsonAsync("/api/v1/breaks", command2);
        
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Match(s => s == HttpStatusCode.Conflict || s == HttpStatusCode.BadRequest || s == HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task GetBreaksByShift_SupportsPagination()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Pag");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId);
        var shiftId = await CreateTestShift(client, companyId, workScheduleId, "Pag Shift", 8, 16);

        // We create breaks that don't overlap to not trigger invariants
        for (int i = 1; i <= 5; i++)
        {
            var start = 8 + i;
            await client.PostAsJsonAsync("/api/v1/breaks", new CreateBreakCommand 
            { 
                ShiftId = shiftId,
                CompanyId = companyId,
                Name = $"Break-{i}", 
                StartTime = new TimeOnly(start, 0, 0), 
                EndTime = new TimeOnly(start, 15, 0),
                DurationMinutes = 15
            });
        }

        var getResponse = await client.GetAsync($"/api/v1/breaks/shift/{shiftId}?pageNumber=1&pageSize=3");
        var result = await getResponse.ReadWrappedResponseAsync<PagedResult<BreakDto>>();
        
        result!.Items.Count.Should().Be(3);
        result.Pagination.PageNumber.Should().Be(1);
        
        var getResponse2 = await client.GetAsync($"/api/v1/breaks/shift/{shiftId}?pageNumber=2&pageSize=3");
        var result2 = await getResponse2.ReadWrappedResponseAsync<PagedResult<BreakDto>>();
        
        result2!.Items.Count.Should().BeGreaterThan(0);
        result2.Pagination.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task SearchBreaks_FiltersByName()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Search");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId);
        var shiftId = await CreateTestShift(client, companyId, workScheduleId);

        await client.PostAsJsonAsync("/api/v1/breaks", new CreateBreakCommand { ShiftId = shiftId, CompanyId = companyId, Name = "Lunch Break", DurationMinutes = 60, StartTime = new TimeOnly(12,0), EndTime = new TimeOnly(13,0) });
        await client.PostAsJsonAsync("/api/v1/breaks", new CreateBreakCommand { ShiftId = shiftId, CompanyId = companyId, Name = "Coffee Break", DurationMinutes = 15, StartTime = new TimeOnly(15,0), EndTime = new TimeOnly(15,15) });

        var getResponse = await client.GetAsync($"/api/v1/breaks?searchTerm=Lunch");
        var result = await getResponse.ReadWrappedResponseAsync<PagedResult<BreakListDto>>();
        
        result!.Items.Should().Contain(x => x.Name == "Lunch Break");
        result.Items.Should().NotContain(x => x.Name == "Coffee Break");
    }

    [Fact]
    public async Task UpdateBreak_ValidData_UpdatesCorrectly()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Upd");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId);
        var shiftId = await CreateTestShift(client, companyId, workScheduleId);

        var createCommand = new CreateBreakCommand { CompanyId = companyId, ShiftId = shiftId, Name = "Old Break", DurationMinutes = 30, StartTime = new TimeOnly(12,0), EndTime = new TimeOnly(13,0) };
        var response = await client.PostAsJsonAsync("/api/v1/breaks", createCommand);
        var id = await response.ReadWrappedResponseAsync<Guid>();

        var updateCommand = new UpdateBreakCommand
        {
            BreakId = id,
            Name = "New Break",
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(13, 0),
            DurationMinutes = 60
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/breaks/{id}", updateCommand);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/breaks/{id}");
        var @break = await getResponse.ReadWrappedResponseAsync<BreakDetailsDto>();
        @break!.Name.Should().Be("New Break");
    }

    [Fact]
    public async Task DeleteBreak_RemovesBreak()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "Del");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId);
        var shiftId = await CreateTestShift(client, companyId, workScheduleId);

        var command = new CreateBreakCommand { CompanyId = companyId, ShiftId = shiftId, Name = "To Delete", DurationMinutes = 15, StartTime = new TimeOnly(12,0), EndTime = new TimeOnly(13,0) };
        var response = await client.PostAsJsonAsync("/api/v1/breaks", command);
        var id = await response.ReadWrappedResponseAsync<Guid>();

        var delResponse = await client.DeleteAsync($"/api/v1/breaks/{id}");
        delResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/breaks/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateDeactivateRestore_ChangesState()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = await CreateTestCompany(client, "State");
        var workScheduleId = await CreateTestWorkSchedule(client, companyId);
        var shiftId = await CreateTestShift(client, companyId, workScheduleId);

        var command = new CreateBreakCommand { CompanyId = companyId, ShiftId = shiftId, Name = "State Break", DurationMinutes = 15, StartTime = new TimeOnly(12,0), EndTime = new TimeOnly(13,0) };
        var response = await client.PostAsJsonAsync("/api/v1/breaks", command);
        var id = await response.ReadWrappedResponseAsync<Guid>();

        // Deactivate
        var deactResponse = await client.PostAsync($"/api/v1/breaks/{id}/deactivate", null);
        deactResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/v1/breaks/{id}");
        var @break = await getResponse.ReadWrappedResponseAsync<BreakDetailsDto>();
        @break!.IsActive.Should().BeFalse();

        // Activate
        var actResponse = await client.PostAsync($"/api/v1/breaks/{id}/activate", null);
        actResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        getResponse = await client.GetAsync($"/api/v1/breaks/{id}");
        @break = await getResponse.ReadWrappedResponseAsync<BreakDetailsDto>();
        @break!.IsActive.Should().BeTrue();

        // Delete (Soft Delete)
        await client.DeleteAsync($"/api/v1/breaks/{id}");

        // Restore
        var restoreResponse = await client.PostAsync($"/api/v1/breaks/{id}/restore", null);
        restoreResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        getResponse = await client.GetAsync($"/api/v1/breaks/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
