using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class DeactivateEmployeeCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
}

public class DeactivateEmployeeCommandValidator : AbstractValidator<DeactivateEmployeeCommand>
{
    public DeactivateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class DeactivateEmployeeCommandHandler : IRequestHandler<DeactivateEmployeeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        try
        {
            employee.Deactivate(isAdmin: false); // Requires proper context for isAdmin if needed

            return Result.Success();
        }
        catch (SmartAttendance.Domain.Exceptions.InvalidEmployeeException ex)
        {
            return Result.Failure(new[] { ex.Message });
        }
    }
}

