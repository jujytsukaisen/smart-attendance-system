using FluentValidation;
using SmartAttendance.Application.Features.Attendances.Commands;

namespace SmartAttendance.Application.Features.Attendances.Validators;

public class CheckInCommandValidator : AbstractValidator<CheckInCommand>
{
    public CheckInCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90.0, 90.0);
        RuleFor(x => x.Longitude).InclusiveBetween(-180.0, 180.0);
        RuleFor(x => x.CheckInTime).NotEmpty();
    }
}
