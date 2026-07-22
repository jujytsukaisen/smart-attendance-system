using FluentValidation;
using SmartAttendance.Application.Features.Attendances.Commands;

namespace SmartAttendance.Application.Features.Attendances.Validators;

public class CheckOutCommandValidator : AbstractValidator<CheckOutCommand>
{
    public CheckOutCommandValidator()
    {
        RuleFor(x => x.AttendanceId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90.0, 90.0);
        RuleFor(x => x.Longitude).InclusiveBetween(-180.0, 180.0);
        RuleFor(x => x.CheckOutTime).NotEmpty();
    }
}
