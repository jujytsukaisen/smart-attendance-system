using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using SmartAttendance.Application.Common.Helpers;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Features.Attendances.Commands;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
using Xunit;

namespace SmartAttendance.UnitTests.Application.Features.Attendances.Commands;

public class RecordScanCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IGeoLocationService> _geoLocationServiceMock;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _shiftId = Guid.NewGuid();
    private readonly Guid _policyId = Guid.NewGuid();

    public RecordScanCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _geoLocationServiceMock = new Mock<IGeoLocationService>();

        _currentUserServiceMock.Setup(c => c.TenantId).Returns(_tenantId.ToString());

        var emptyEmployees = new List<Employee>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Employees).Returns(emptyEmployees.Object);
    }

    [Fact]
    public void AttendanceStateHelper_ComputeState_ShouldReturnNotCheckedIn_WhenEventsEmpty()
    {
        // Arrange
        var events = new List<AttendanceEvent>();

        // Act
        var state = AttendanceStateHelper.ComputeState(events);

        // Assert
        state.Should().Be(EmployeeAttendanceState.NotCheckedIn);
    }

    [Fact]
    public void AttendanceStateHelper_ComputeState_ShouldReturnWorking_AfterCheckIn()
    {
        // Arrange
        var events = new List<AttendanceEvent>
        {
            AttendanceEvent.Create(_tenantId, Guid.NewGuid(), _employeeId, DateTime.UtcNow, AttendanceEventType.CheckIn)
        };

        // Act
        var state = AttendanceStateHelper.ComputeState(events);

        // Assert
        state.Should().Be(EmployeeAttendanceState.Working);
    }

    [Fact]
    public void AttendanceStateHelper_ComputeState_ShouldReturnOnBreak_AfterBreakStart()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var events = new List<AttendanceEvent>
        {
            AttendanceEvent.Create(_tenantId, Guid.NewGuid(), _employeeId, now.AddHours(-2), AttendanceEventType.CheckIn),
            AttendanceEvent.Create(_tenantId, Guid.NewGuid(), _employeeId, now.AddHours(-1), AttendanceEventType.BreakStart),
        };

        // Act
        var state = AttendanceStateHelper.ComputeState(events);

        // Assert
        state.Should().Be(EmployeeAttendanceState.OnBreak);
    }

    [Fact]
    public void AttendanceStateHelper_ComputeState_ShouldReturnWorking_AfterBreakEnd()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var events = new List<AttendanceEvent>
        {
            AttendanceEvent.Create(_tenantId, Guid.NewGuid(), _employeeId, now.AddHours(-3), AttendanceEventType.CheckIn),
            AttendanceEvent.Create(_tenantId, Guid.NewGuid(), _employeeId, now.AddHours(-2), AttendanceEventType.BreakStart),
            AttendanceEvent.Create(_tenantId, Guid.NewGuid(), _employeeId, now.AddHours(-1), AttendanceEventType.BreakEnd),
        };

        // Act
        var state = AttendanceStateHelper.ComputeState(events);

        // Assert
        state.Should().Be(EmployeeAttendanceState.Working);
    }

    [Fact]
    public void AttendanceStateHelper_ComputeState_ShouldReturnFinished_AfterCheckOut()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var events = new List<AttendanceEvent>
        {
            AttendanceEvent.Create(_tenantId, Guid.NewGuid(), _employeeId, now.AddHours(-4), AttendanceEventType.CheckIn),
            AttendanceEvent.Create(_tenantId, Guid.NewGuid(), _employeeId, now, AttendanceEventType.CheckOut),
        };

        // Act
        var state = AttendanceStateHelper.ComputeState(events);

        // Assert
        state.Should().Be(EmployeeAttendanceState.Finished);
    }

    [Fact]
    public async Task Handle_FirstScan_ShouldCreateAttendanceWithCheckInEvent()
    {
        // Arrange
        var employee = (Employee)RuntimeHelpers.GetUninitializedObject(typeof(Employee));
        typeof(Employee).GetProperty("Id")!.SetValue(employee, _employeeId);
        typeof(Employee).GetProperty("CompanyId")!.SetValue(employee, _tenantId);
        var employees = new List<Employee> { employee }.AsQueryable().BuildMockDbSet();

        var shift = (Shift)RuntimeHelpers.GetUninitializedObject(typeof(Shift));
        typeof(Shift).GetProperty("Id")!.SetValue(shift, _shiftId);
        typeof(Shift).GetProperty("CompanyId")!.SetValue(shift, _tenantId);
        var shifts = new List<Shift> { shift }.AsQueryable().BuildMockDbSet();

        var policy = (AttendancePolicy)RuntimeHelpers.GetUninitializedObject(typeof(AttendancePolicy));
        typeof(AttendancePolicy).GetProperty("Id")!.SetValue(policy, _policyId);
        typeof(AttendancePolicy).GetProperty("CompanyId")!.SetValue(policy, _tenantId);
        var policies = new List<AttendancePolicy> { policy }.AsQueryable().BuildMockDbSet();

        var deviceId = Guid.NewGuid();
        var device = (AttendanceDevice)RuntimeHelpers.GetUninitializedObject(typeof(AttendanceDevice));
        typeof(AttendanceDevice).GetProperty("Id")!.SetValue(device, deviceId);
        typeof(AttendanceDevice).GetProperty("CompanyId")!.SetValue(device, _tenantId);
        var devices = new List<AttendanceDevice> { device }.AsQueryable().BuildMockDbSet();
        var branches = new List<Branch>().AsQueryable().BuildMockDbSet();
        var attendances = new List<SmartAttendance.Domain.Entities.Attendance>().AsQueryable().BuildMockDbSet();
        var attendanceEvents = new List<AttendanceEvent>().AsQueryable().BuildMockDbSet();

        _contextMock.Setup(c => c.Employees).Returns(employees.Object);
        _contextMock.Setup(c => c.Branches).Returns(branches.Object);
        _contextMock.Setup(c => c.Shifts).Returns(shifts.Object);
        _contextMock.Setup(c => c.AttendancePolicies).Returns(policies.Object);
        _contextMock.Setup(c => c.AttendanceDevices).Returns(devices.Object);
        _contextMock.Setup(c => c.Attendances).Returns(attendances.Object);
        _contextMock.Setup(c => c.AttendanceEvents).Returns(attendanceEvents.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new RecordScanCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _geoLocationServiceMock.Object);

        var command = new RecordScanCommand
        {
            EmployeeId = _employeeId,
            ShiftId = _shiftId,
            AttendancePolicyId = _policyId,
            CheckInMethod = 1,
            Timestamp = DateTime.UtcNow,
            IsExplicitCheckOut = false
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.ActionTaken.Should().Be(AttendanceEventType.CheckIn.ToString());
        result.Data.CurrentState.Should().Be(EmployeeAttendanceState.Working.ToString());

        attendances.Verify(a => a.AddAsync(It.IsAny<SmartAttendance.Domain.Entities.Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
        attendanceEvents.Verify(e => e.AddAsync(It.IsAny<AttendanceEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
