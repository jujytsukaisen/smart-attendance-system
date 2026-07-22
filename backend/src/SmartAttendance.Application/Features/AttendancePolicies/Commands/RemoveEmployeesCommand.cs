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

public class RemoveEmployeesCommand : IRequest<Result>
{
    public Guid AttendancePolicyId { get; set; }
    public List<Guid> EmployeeIds { get; set; } = new();
}

public class RemoveEmployeesCommandValidator : AbstractValidator<RemoveEmployeesCommand>
{
    public RemoveEmployeesCommandValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
        RuleFor(x => x.EmployeeIds).NotEmpty();
    }
}

public class RemoveEmployeesCommandHandler : IRequestHandler<RemoveEmployeesCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveEmployeesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveEmployeesCommand request, CancellationToken cancellationToken)
    {
        var currentPolicy = await _context.AttendancePolicies.FindAsync(new object[] { request.AttendancePolicyId }, cancellationToken);
        if (currentPolicy == null) return Result.Failure(new[] { "Attendance policy not found." });

        var defaultPolicy = await _context.AttendancePolicies
            .FirstOrDefaultAsync(p => p.CompanyId == currentPolicy.CompanyId && p.IsDefault && p.Id != request.AttendancePolicyId, cancellationToken);

        if (defaultPolicy == null)
            return Result.Failure(new[] { "Cannot remove employees from policy without a default fallback policy in the company." });

        var employees = await _context.Employees
            .Where(e => request.EmployeeIds.Contains(e.Id) && e.AttendancePolicyId == request.AttendancePolicyId)
            .ToListAsync(cancellationToken);

        foreach (var employee in employees)
        {
            employee.AssignAttendancePolicy(defaultPolicy.Id);
        }

        return Result.Success();
    }
}

