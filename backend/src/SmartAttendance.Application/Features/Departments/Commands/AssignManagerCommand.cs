using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class AssignManagerCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
    public Guid ManagerId { get; set; }
}

public class AssignManagerCommandValidator : AbstractValidator<AssignManagerCommand>
{
    public AssignManagerCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
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
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
        if (department == null) return Result.Failure(new[] { "Department not found." });
        
        department.AssignManager(request.ManagerId);
        
        _context.Departments.Update(department);

        return Result.Success();
    }
}

