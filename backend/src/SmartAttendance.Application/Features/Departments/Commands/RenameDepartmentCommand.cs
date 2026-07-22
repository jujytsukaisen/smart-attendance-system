using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class RenameDepartmentCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
    public string NewName { get; set; } = string.Empty;
}

public class RenameDepartmentCommandValidator : AbstractValidator<RenameDepartmentCommand>
{
    public RenameDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.NewName).NotEmpty().MaximumLength(100);
    }
}

public class RenameDepartmentCommandHandler : IRequestHandler<RenameDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RenameDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RenameDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
        if (department == null) return Result.Failure(new[] { "Department not found." });
        
        department.Rename(request.NewName);
        
        _context.Departments.Update(department);

        return Result.Success();
    }
}

