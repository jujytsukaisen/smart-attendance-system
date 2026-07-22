using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.WorkSchedules.Commands;

public class AssignEmployeesCommand : IRequest<Result>
{
    public Guid WorkScheduleId { get; set; }
    public List<Guid> EmployeeIds { get; set; } = new();
}

public class AssignEmployeesCommandValidator : AbstractValidator<AssignEmployeesCommand>
{
    public AssignEmployeesCommandValidator()
    {
        RuleFor(x => x.WorkScheduleId).NotEmpty();
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
        var schedule = await _context.WorkSchedules
            .Include(w => w.Employees)
            .FirstOrDefaultAsync(w => w.Id == request.WorkScheduleId, cancellationToken);

        if (schedule == null)
        {
            return Result.Failure(new[] { "Work schedule not found." });
        }

        var employees = await _context.Employees
            .Where(e => request.EmployeeIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (employees.Count != request.EmployeeIds.Count)
        {
            return Result.Failure(new[] { "One or more employees not found." });
        }

        foreach (var employee in employees)
        {
            schedule.AssignEmployee(employee);
        }

        return Result.Success();
    }
}

