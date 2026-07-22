using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Departments.Commands;

public class CreateDepartmentCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
            return Result<Guid>.Failure(new[] { "Company not found." });

        var branchExists = await _context.Branches.AnyAsync(b => b.Id == request.BranchId && b.CompanyId == request.CompanyId, cancellationToken);
        if (!branchExists)
            return Result<Guid>.Failure(new[] { "Branch not found or does not belong to the specified company." });

        var duplicateName = await _context.Departments.AnyAsync(d => d.BranchId == request.BranchId && d.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        if (duplicateName)
            return Result<Guid>.Failure(new[] { "Department name already exists in this branch." });

        var duplicateCode = await _context.Departments.AnyAsync(d => d.CompanyId == request.CompanyId && d.Code.ToLower() == request.Code.ToLower(), cancellationToken);
        if (duplicateCode)
            return Result<Guid>.Failure(new[] { "Department code already exists in this company." });

        var department = SmartAttendance.Domain.Entities.Department.Create(
            request.CompanyId,
            request.BranchId,
            request.Name,
            request.Code,
            request.Description);

        _context.Departments.Add(department);

        return Result<Guid>.Success(department.Id);
    }
}

