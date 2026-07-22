using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class RestoreDepartmentCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
}

public class RestoreDepartmentCommandValidator : AbstractValidator<RestoreDepartmentCommand>
{
    public RestoreDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}

public class RestoreDepartmentCommandHandler : IRequestHandler<RestoreDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RestoreDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RestoreDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);

        if (department == null)
            return Result.Failure(new[] { "Department not found." });

        if (!department.IsDeleted)
            return Result.Failure(new[] { "Department is not deleted." });

        department.Restore();

        return Result.Success();
    }
}
