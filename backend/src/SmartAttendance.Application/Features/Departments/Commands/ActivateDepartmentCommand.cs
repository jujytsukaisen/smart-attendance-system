using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class ActivateDepartmentCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
}

public class ActivateDepartmentCommandValidator : AbstractValidator<ActivateDepartmentCommand>
{
    public ActivateDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}

public class ActivateDepartmentCommandHandler : IRequestHandler<ActivateDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FindAsync(new object[] { request.DepartmentId }, cancellationToken);

        if (department == null)
            return Result.Failure(new[] { "Department not found." });

        department.Activate();

        return Result.Success();
    }
}

