using System;
using FluentValidation.TestHelper;
using SmartAttendance.Application.Features.Shifts.Commands;
using Xunit;

namespace SmartAttendance.UnitTests.Application.Features.Shifts.Commands;

public class CreateShiftCommandValidatorTests
{
    private readonly CreateShiftCommandValidator _validator;

    public CreateShiftCommandValidatorTests()
    {
        _validator = new CreateShiftCommandValidator();
    }

    [Fact]
    public void Should_HaveError_When_CompanyIdIsEmpty()
    {
        var command = new CreateShiftCommand { CompanyId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CompanyId);
    }

    [Fact]
    public void Should_HaveError_When_WorkScheduleIdIsEmpty()
    {
        var command = new CreateShiftCommand { WorkScheduleId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.WorkScheduleId);
    }

    [Fact]
    public void Should_HaveError_When_NameIsTooLong()
    {
        var command = new CreateShiftCommand { Name = new string('A', 101) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_MaxHoursLessThanMinHours()
    {
        var command = new CreateShiftCommand
        {
            MinimumWorkingHours = 8,
            MaximumWorkingHours = 7
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MaximumWorkingHours)
              .WithErrorMessage("Maximum working hours must be greater than or equal to minimum working hours.");
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        var command = new CreateShiftCommand
        {
            CompanyId = Guid.NewGuid(),
            WorkScheduleId = Guid.NewGuid(),
            Name = "Valid Shift",
            Code = "SH-VAL-01",
            StartTime = new TimeOnly(9, 0, 0),
            EndTime = new TimeOnly(17, 0, 0),
            MinimumWorkingHours = 7,
            MaximumWorkingHours = 8
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
