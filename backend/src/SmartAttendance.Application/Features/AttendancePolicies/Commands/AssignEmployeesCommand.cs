using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.AttendancePolicies.Commands;

public class AssignEmployeesCommand : IRequest<Result>
{
    public Guid AttendancePolicyId { get; set; }
    public List<Guid> EmployeeIds { get; set; } = new();
}

public class AssignEmployeesCommandValidator : AbstractValidator<AssignEmployeesCommand>
{
    public AssignEmployeesCommandValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
        RuleFor(x => x.EmployeeIds).NotEmpty();
    }
}

public class AssignEmployeesCommandHandler : IRequestHandler<AssignEmployeesCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignEmployeesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignEmployeesCommand request, CancellationToken cancellationToken)
    {
        var policyExists = await _context.AttendancePolicies.AnyAsync(p => p.Id == request.AttendancePolicyId, cancellationToken);
        if (!policyExists) return Result.Failure(new[] { "Attendance policy not found." });

        var employees = await _context.Employees
            .Where(e => request.EmployeeIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        foreach (var employee in employees)
        {
            employee.AssignAttendancePolicy(request.AttendancePolicyId);
        }

        return Result.Success();
    }
}

