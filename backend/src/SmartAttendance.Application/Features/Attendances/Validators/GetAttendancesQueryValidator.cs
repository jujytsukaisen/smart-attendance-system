using FluentValidation;
using SmartAttendance.Application.Features.Attendances.Queries;

namespace SmartAttendance.Application.Features.Attendances.Validators;

public class GetAttendancesQueryValidator : AbstractValidator<GetAttendancesQuery>
{
    public GetAttendancesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}
