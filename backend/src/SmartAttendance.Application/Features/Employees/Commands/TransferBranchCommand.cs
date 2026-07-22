using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class TransferBranchCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
    public Guid NewBranchId { get; set; }
}

public class TransferBranchCommandValidator : AbstractValidator<TransferBranchCommand>
{
    public TransferBranchCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.NewBranchId).NotEmpty();
    }
}

public class TransferBranchCommandHandler : IRequestHandler<TransferBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public TransferBranchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(TransferBranchCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        var branchExists = await _context.Branches.AnyAsync(b => b.Id == request.NewBranchId && b.CompanyId == employee.CompanyId, cancellationToken);
        if (!branchExists)
            return Result.Failure(new[] { "Branch not found or does not belong to the employee's company." });

        try
        {
            employee.TransferToBranch(request.NewBranchId);

            return Result.Success();
        }
        catch (SmartAttendance.Domain.Exceptions.InvalidEmployeeException ex)
        {
            return Result.Failure(new[] { ex.Message });
        }
    }
}

