using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class RemoveManagerCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
}

public class RemoveManagerCommandValidator : AbstractValidator<RemoveManagerCommand>
{
    public RemoveManagerCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}

public class RemoveManagerCommandHandler : IRequestHandler<RemoveManagerCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveManagerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveManagerCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
        if (department == null) return Result.Failure(new[] { "Department not found." });
        
        department.RemoveManager();
        
        _context.Departments.Update(department);

        return Result.Success();
    }
}

