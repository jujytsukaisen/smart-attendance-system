using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class AssignManagerCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
    public Guid ManagerId { get; set; }
}

public class AssignManagerCommandValidator : AbstractValidator<AssignManagerCommand>
{
    public AssignManagerCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.ManagerId).NotEmpty();
    }
}

public class AssignManagerCommandHandler : IRequestHandler<AssignManagerCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignManagerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignManagerCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        var managerExists = await _context.Employees.AnyAsync(e => e.Id == request.ManagerId && e.CompanyId == employee.CompanyId, cancellationToken);
        if (!managerExists)
            return Result.Failure(new[] { "Manager not found or does not belong to the employee's company." });

        try
        {
            employee.AssignManager(request.ManagerId);

            return Result.Success();
        }
        catch (SmartAttendance.Domain.Exceptions.InvalidEmployeeException ex)
        {
            return Result.Failure(new[] { ex.Message });
        }
    }
}

