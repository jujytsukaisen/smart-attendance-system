using FluentValidation;
using SmartAttendance.Application.Features.Attendances.Queries;

namespace SmartAttendance.Application.Features.Attendances.Validators;

public class GetAttendanceByIdQueryValidator : AbstractValidator<GetAttendanceByIdQuery>
{
    public GetAttendanceByIdQueryValidator()
    {
        RuleFor(x => x.AttendanceId).NotEmpty();
    }
}
