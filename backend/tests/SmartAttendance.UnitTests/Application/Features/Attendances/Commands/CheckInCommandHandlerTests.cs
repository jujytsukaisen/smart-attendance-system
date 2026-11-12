using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Attendances.Commands;
using SmartAttendance.Domain.Entities;
using Xunit;

namespace SmartAttendance.UnitTests.Application.Features.Attendance.Commands;

public class CheckInCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IFaceRecognitionService> _faceRecognitionServiceMock;
    private readonly Mock<IGeoLocationService> _geoLocationServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<DbSet<SmartAttendance.Domain.Entities.Attendance>> _attendanceDbSetMock;
    private readonly CheckInCommandHandler _handler;

    public CheckInCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _faceRecognitionServiceMock = new Mock<IFaceRecognitionService>();
        _geoLocationServiceMock = new Mock<IGeoLocationService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _attendanceDbSetMock = new Mock<DbSet<SmartAttendance.Domain.Entities.Attendance>>();

        var emptyPolicies = new List<SmartAttendance.Domain.Entities.AttendancePolicy>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.AttendancePolicies).Returns(emptyPolicies.Object);

        var emptyEmployees = new List<SmartAttendance.Domain.Entities.Employee>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Employees).Returns(emptyEmployees.Object);

        var emptyBranches = new List<SmartAttendance.Domain.Entities.Branch>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Branches).Returns(emptyBranches.Object);

        var emptyDevices = new List<SmartAttendance.Domain.Entities.AttendanceDevice>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.AttendanceDevices).Returns(emptyDevices.Object);

        var emptyShifts = new List<SmartAttendance.Domain.Entities.Shift>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Shifts).Returns(emptyShifts.Object);

        var attendanceEvents = new List<AttendanceEvent>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.AttendanceEvents).Returns(attendanceEvents.Object);

        _currentUserServiceMock.Setup(c => c.TenantId).Returns(Guid.NewGuid().ToString());
        _contextMock.Setup(c => c.Attendances).Returns(_attendanceDbSetMock.Object);


        _handler = new CheckInCommandHandler(
            _contextMock.Object, 
            _faceRecognitionServiceMock.Object,
            _geoLocationServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddAttendanceAndSaveChanges_WhenCommandIsValid()
    {
        // Arrange
        var command = new CheckInCommand
        {
            EmployeeId = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            AttendancePolicyId = Guid.NewGuid(),
            CheckInMethod = 1, // FaceRecognition
            Latitude = 40.7128,
            Longitude = -74.0060,
            CheckInTime = DateTime.UtcNow
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data!.Status.Should().Be("CheckedIn");
        
        _attendanceDbSetMock.Verify(d => d.AddAsync(It.IsAny<SmartAttendance.Domain.Entities.Attendance>(), It.IsAny<CancellationToken>()), Times.Once);

    }
}

