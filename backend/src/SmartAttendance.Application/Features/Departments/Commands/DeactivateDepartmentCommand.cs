using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class DeactivateDepartmentCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
}

public class DeactivateDepartmentCommandValidator : AbstractValidator<DeactivateDepartmentCommand>
{
    public DeactivateDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}

public class DeactivateDepartmentCommandHandler : IRequestHandler<DeactivateDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FindAsync(new object[] { request.DepartmentId }, cancellationToken);

        if (department == null)
            return Result.Failure(new[] { "Department not found." });

        department.Deactivate();

        return Result.Success();
    }
}

