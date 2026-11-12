using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class UpdateDepartmentCommand : IRequest<Result>
{
    public Guid DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool RequiresLocationValidation { get; set; } = true;
}

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.DepartmentId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FindAsync(new object[] { request.DepartmentId }, cancellationToken);

        if (department == null)
            return Result.Failure(new[] { "Department not found." });

        var duplicateName = await _context.Departments.AnyAsync(d => d.Id != request.DepartmentId && d.BranchId == department.BranchId && d.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        if (duplicateName)
            return Result.Failure(new[] { "Department name already exists in this branch." });

        department.Rename(request.Name);
        department.ChangeDescription(request.Description);
        department.SetLocationValidationRequirement(request.RequiresLocationValidation);

        return Result.Success();
    }
}



