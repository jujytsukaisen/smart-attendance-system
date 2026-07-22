using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class TransferDepartmentCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
    public Guid NewDepartmentId { get; set; }
}

public class TransferDepartmentCommandValidator : AbstractValidator<TransferDepartmentCommand>
{
    public TransferDepartmentCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.NewDepartmentId).NotEmpty();
    }
}

public class TransferDepartmentCommandHandler : IRequestHandler<TransferDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public TransferDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(TransferDepartmentCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        var departmentExists = await _context.Departments.AnyAsync(d => d.Id == request.NewDepartmentId && d.BranchId == employee.BranchId, cancellationToken);
        if (!departmentExists)
            return Result.Failure(new[] { "Department not found or does not belong to the employee's current branch." });

        try
        {
            employee.TransferToDepartment(request.NewDepartmentId);

            return Result.Success();
        }
        catch (SmartAttendance.Domain.Exceptions.InvalidEmployeeException ex)
        {
            return Result.Failure(new[] { ex.Message });
        }
    }
}

