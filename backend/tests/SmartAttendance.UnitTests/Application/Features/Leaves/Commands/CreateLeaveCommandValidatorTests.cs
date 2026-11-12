using System;
using FluentValidation.TestHelper;
using SmartAttendance.Application.Features.Leaves.Commands;
using Xunit;

namespace SmartAttendance.UnitTests.Application.Features.Leaves.Commands;

public class CreateLeaveCommandValidatorTests
{
    private readonly CreateLeaveCommandValidator _validator;

    public CreateLeaveCommandValidatorTests()
    {
        _validator = new CreateLeaveCommandValidator();
    }

    [Fact]
    public void Should_HaveError_When_EmployeeIdIsEmpty()
    {
        var command = new CreateLeaveCommand { EmployeeId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Should_HaveError_When_EndDateIsBeforeStartDate()
    {
        var command = new CreateLeaveCommand
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1)
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
              .WithErrorMessage("EndDate must be greater than or equal to StartDate.");
    }

    [Fact]
    public void Should_HaveError_When_ReasonExceedsMaxLength()
    {
        var command = new CreateLeaveCommand { Reason = new string('A', 501) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Should_HaveError_When_RequiredFieldsAreMissing()
    {
        var command = new CreateLeaveCommand(); // Empty command
        var result = _validator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        result.ShouldHaveValidationErrorFor(x => x.LeaveType);
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        var command = new CreateLeaveCommand
        {
            CompanyId = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Reason = "Vacation"
        };
        
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
