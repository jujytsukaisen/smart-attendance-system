using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using SmartAttendance.Application.Features.Attendances.Commands;
using SmartAttendance.Domain.Enums;
using SmartAttendance.IntegrationTests.Common;
using Xunit;
using Xunit.Abstractions;

namespace SmartAttendance.IntegrationTests.Controllers;

public class AttendancesControllerTests : BaseIntegrationTest
{
    private readonly ITestOutputHelper _output;

    public AttendancesControllerTests(CustomWebApplicationFactory factory, ITestOutputHelper output) : base(factory)
    {
        _output = output;
    }

    [Fact]
    public async Task CreateAttendance_ShouldReturnSuccessAndCreateRecord()
    {
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");

        var companyCommand = new 
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
        var companyResponse = await client.PostAsJsonAsync("/api/v1/companies", companyCommand);
        companyResponse.EnsureSuccessStatusCode();
        var companyId = await companyResponse.ReadWrappedResponseAsync<Guid>();

        var branchCommand = new 
        {
            CompanyId = companyId,
            Name = "Main Branch", Code = "IT-BR", AddressStreet = "123 St", AddressCity = "City", AddressState = "TS", AddressCountry = "Land", AddressZipCode = "12345", PhoneNumber = "+1234567891", Email = "branch@integration.com", Latitude = 40.71, Longitude = -74.00, AllowedRadiusMeters = 100, TimeZone = "America/New_York", IsHeadOffice = true
        };
        var branchResponse = await client.PostAsJsonAsync("/api/v1/branches", branchCommand);
        branchResponse.EnsureSuccessStatusCode();
        var branchId = await branchResponse.ReadWrappedResponseAsync<Guid>();

        var deptCommand = new 
        {
            CompanyId = companyId, BranchId = branchId, Name = "IT", Code = "IT-DEPT", Description = "IT Dept"
        };
        var deptResponse = await client.PostAsJsonAsync("/api/v1/departments", deptCommand);
        deptResponse.EnsureSuccessStatusCode();
        var deptId = await deptResponse.ReadWrappedResponseAsync<Guid>();

        var polCommand = new 
        {
            CompanyId = companyId, Name = "Standard Policy", Code = "SP01", Description = "Test Policy", AttendanceMode = AttendanceMode.Remote, IsActive = true, CommunicationProtocol = 1, RequireFaceRecognition = false, RequireGPS = false, RequireApprovedDevice = false, RequireCompanyNetwork = false, AllowMobileCheckIn = true, AllowDesktopCheckIn = true, AllowWebCheckIn = true, AllowOfflineAttendance = false, AllowedRadiusMeters = 100, AllowMultipleCheckIn = false, AllowManualAttendance = false, RequireManagerApproval = false, MaximumDistanceMeters = 1000
        };
        var polResponse = await client.PostAsJsonAsync("/api/v1/attendancepolicies", polCommand);
        polResponse.EnsureSuccessStatusCode();
        var polId = await polResponse.ReadWrappedResponseAsync<Guid>();

        var scheduleCommand = new 
        {
            CompanyId = companyId, Name = "Standard Morning Schedule", Description = "Standard 40 hours", IsFlexible = false, WeeklyWorkingHours = 40, GracePeriodMinutes = 15, AllowEarlyCheckInMinutes = 30, AllowLateCheckOutMinutes = 30
        };
        var scheduleResponse = await client.PostAsJsonAsync("/api/v1/workschedules", scheduleCommand);
        scheduleResponse.EnsureSuccessStatusCode();
        var scheduleId = await scheduleResponse.ReadWrappedResponseAsync<Guid>();

        var shiftCommand = new 
        {
            CompanyId = companyId, WorkScheduleId = scheduleId, Name = "Morning Shift", Code = "MS01", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), PolicyId = polId, IsFlexible = false, ShiftType = 0, MinimumWorkingHours = 7, MaximumWorkingHours = 8
        };
        var shiftResponse = await client.PostAsJsonAsync("/api/v1/shifts", shiftCommand);
        
        if (!shiftResponse.IsSuccessStatusCode)
        {
            var err = await shiftResponse.Content.ReadAsStringAsync();
            _output.WriteLine("Shift creation failed: " + err);
            shiftResponse.EnsureSuccessStatusCode();
        }
        
        var shiftId = await shiftResponse.ReadWrappedResponseAsync<Guid>();

        var deviceCommand = new 
        {
            CompanyId = companyId, Name = "Main Gate", DeviceIdentifier = "DEV01", PolicyId = polId, BranchId = branchId, Code = "DEV01", DeviceType = 1, IpAddress = "127.0.0.1", SerialNumber = "1234", IsActive = true, CommunicationProtocol = 1
        };
        var deviceResponse = await client.PostAsJsonAsync("/api/v1/attendancedevices", deviceCommand);
        
        if (!deviceResponse.IsSuccessStatusCode)
        {
            var err = await deviceResponse.Content.ReadAsStringAsync();
            _output.WriteLine("Device creation failed: " + err);
            deviceResponse.EnsureSuccessStatusCode();
        }
        
        var deviceId = await deviceResponse.ReadWrappedResponseAsync<Guid>();

        var empCommand = new 
        {
            CompanyId = companyId, BranchId = branchId, DepartmentId = deptId, AttendancePolicyId = polId, EmployeeNumber = "EMP-001", FirstName = "John", LastName = "Doe", Email = "john.doe@integration.com", PhoneNumber = "+19876543210", Street = "456 Emp St", City = "Empville", State = "ES", ZipCode = "54321", Country = "Empland", DateOfBirth = new DateTime(1990, 1, 1), HireDate = new DateTime(2023, 1, 1), JobTitle = "SE", Gender = SmartAttendance.Domain.Enums.Gender.Male
        };
        var empResponse = await client.PostAsJsonAsync("/api/v1/employees", empCommand);
        empResponse.EnsureSuccessStatusCode();
        var empId = await empResponse.ReadWrappedResponseAsync<Guid>();

        var command = new CreateAttendanceCommand
        {
            EmployeeId = empId,
            AttendancePolicyId = polId,
            ShiftId = shiftId,
            AttendanceDeviceId = deviceId,
            CheckInTime = DateTime.UtcNow,
            CheckInMethod = CheckInMethod.Manual
        };

        var response = await client.PostAsJsonAsync("/api/v1/attendances", command);
        response.EnsureSuccessStatusCode();
        
        var createdId = await response.ReadWrappedResponseAsync<Guid>();
        createdId.Should().NotBeEmpty();
    }
}
