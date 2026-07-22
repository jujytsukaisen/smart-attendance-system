using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class ActivateEmployeeCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
}

public class ActivateEmployeeCommandValidator : AbstractValidator<ActivateEmployeeCommand>
{
    public ActivateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class ActivateEmployeeCommandHandler : IRequestHandler<ActivateEmployeeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        try
        {
            employee.Activate();

            return Result.Success();
        }
        catch (SmartAttendance.Domain.Exceptions.InvalidEmployeeException ex)
        {
            return Result.Failure(new[] { ex.Message });
        }
    }
}

