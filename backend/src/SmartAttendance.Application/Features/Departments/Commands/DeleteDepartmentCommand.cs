using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class DeleteDepartmentCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
}

public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FindAsync(new object[] { request.DepartmentId }, cancellationToken);

        if (department == null)
            return Result.Failure(new[] { "Department not found." });

        var hasEmployees = await _context.Employees.AnyAsync(e => e.DepartmentId == request.DepartmentId && !e.IsDeleted, cancellationToken);
        if (hasEmployees)
            return Result.Failure(new[] { "A department cannot be deleted while it still contains employees." });

        department.Delete();

        return Result.Success();
    }
}



