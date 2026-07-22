using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class ChangeBranchCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
    public Guid NewBranchId { get; set; }
}

public class ChangeBranchCommandValidator : AbstractValidator<ChangeBranchCommand>
{
    public ChangeBranchCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.NewBranchId).NotEmpty();
    }
}

public class ChangeBranchCommandHandler : IRequestHandler<ChangeBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeBranchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeBranchCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.DepartmentId, cancellationToken);
        if (department == null) return Result.Failure(new[] { "Department not found." });
        
        department.ChangeBranch(request.NewBranchId);
        
        _context.Departments.Update(department);

        return Result.Success();
    }
}

