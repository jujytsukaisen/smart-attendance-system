using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Leaves.Commands;
using SmartAttendance.Domain.Entities;
using Xunit;

namespace SmartAttendance.UnitTests.Application.Features.Leaves.Commands;

public class CreateLeaveCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<DbSet<Leave>> _leavesDbSetMock;
    private readonly CreateLeaveCommandHandler _handler;

    public CreateLeaveCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _notificationServiceMock = new Mock<INotificationService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _notificationServiceMock.Setup(x => x.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(SmartAttendance.Application.Common.Models.Result.Success());
        _leavesDbSetMock = new Mock<DbSet<Leave>>();

        _contextMock.Setup(c => c.Leaves).Returns(_leavesDbSetMock.Object);

        _handler = new CreateLeaveCommandHandler(_contextMock.Object, _notificationServiceMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddLeaveAndSaveChanges_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateLeaveCommand
        {
            CompanyId = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(2),
            Reason = "Sick"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        
        _leavesDbSetMock.Verify(d => d.AddAsync(It.IsAny<Leave>(), It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(n => n.SendNotificationAsync(command.EmployeeId.ToString(), "Leave Requested", It.IsAny<string>()), Times.Once);
    }
}

