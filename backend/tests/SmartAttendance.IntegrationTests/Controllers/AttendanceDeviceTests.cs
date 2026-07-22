using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.AttendanceDevices.Commands;
using SmartAttendance.Application.Features.AttendanceDevices.DTOs;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Infrastructure.Persistence;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Controllers;

public class AttendanceDeviceTests : BaseIntegrationTest
{
    public AttendanceDeviceTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateAttendanceDevice_ShouldReturnGuid_WhenValid()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var companyId = Guid.NewGuid(); // Simplified for test
        var command = new CreateAttendanceDeviceCommand
        {
            CompanyId = companyId,
            Name = "Test Device",
            Code = "DEV-001",
            DeviceType = DeviceType.WallMounted,
            AllowedRadiusMeters = 50,
            IpAddress = "192.168.1.100",
            Port = 8080,
            CommunicationProtocol = CommunicationProtocol.TCP
        };

        // Seed company
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var company = Company.Create(
                "Test Company", "Test LLC", "REG", "TAX",
                new SmartAttendance.Domain.ValueObjects.Email("test@test.com"),
                new SmartAttendance.Domain.ValueObjects.PhoneNumber("+111"),
                new SmartAttendance.Domain.ValueObjects.Address("A", "B", "C", "D", "E"),
                DateTime.UtcNow);
            company.GetType().GetProperty("Id")!.SetValue(company, companyId);
            context.Companies.Add(company);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/attendancedevices", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var resultId = await response.ReadWrappedResponseAsync<Guid>();
        resultId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAttendanceDevice_ShouldFail_WhenPortIsInvalid()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync("admin@smartattendance.local", "Admin@123456");
        var command = new CreateAttendanceDeviceCommand
        {
            CompanyId = Guid.NewGuid(),
            Name = "Test Device",
            Code = "DEV-001",
            DeviceType = DeviceType.WallMounted,
            AllowedRadiusMeters = 50,
            Port = 99999 // Invalid port
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/attendancedevices", command);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }
}
