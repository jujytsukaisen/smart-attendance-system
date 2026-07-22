using System;
using FluentAssertions;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;
using Xunit;

namespace SmartAttendance.UnitTests.Domain.Entities;

public class LeaveTests
{
    private Leave CreateTestLeave()
    {
        return Leave.Request(
            companyId: Guid.NewGuid(),
            employeeId: Guid.NewGuid(),
            leaveType: LeaveType.Annual,
            startDate: DateTime.UtcNow.Date,
            endDate: DateTime.UtcNow.Date.AddDays(2),
            reason: "Vacation");
    }

    [Fact]
    public void Approve_ShouldUpdateStatusToApproved_WhenPending()
    {
        // Arrange
        var leave = CreateTestLeave();
        var managerId = Guid.NewGuid();

        // Act
        leave.Approve(managerId);

        // Assert
        leave.LeaveStatus.Should().Be(LeaveStatus.Approved);
        leave.ApprovedBy.Should().Be(managerId);
        leave.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reject_ShouldUpdateStatusToRejected_WhenPending()
    {
        // Arrange
        var leave = CreateTestLeave();
        var managerId = Guid.NewGuid();

        // Act
        leave.Reject(managerId);

        // Assert
        leave.LeaveStatus.Should().Be(LeaveStatus.Rejected);
        leave.RejectedBy.Should().Be(managerId);
        leave.RejectedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_ShouldNotThrow_WhenApprovedTwice()
    {
        // Arrange
        var leave = CreateTestLeave();
        var managerId = Guid.NewGuid();
        leave.Approve(managerId);

        // Act
        Action act = () => leave.Approve(managerId);

        // Assert
        // In Leave.cs, if it is already approved, it returns early.
        act.Should().NotThrow();
        leave.LeaveStatus.Should().Be(LeaveStatus.Approved);
    }

    [Fact]
    public void Reject_ShouldThrowInvalidLeaveException_WhenAlreadyApproved()
    {
        // Arrange
        var leave = CreateTestLeave();
        var managerId = Guid.NewGuid();
        leave.Approve(managerId);

        // Act
        Action act = () => leave.Reject(managerId);

        // Assert
        act.Should().Throw<InvalidLeaveException>()
            .WithMessage("Approved leave cannot be rejected.");
    }
}
