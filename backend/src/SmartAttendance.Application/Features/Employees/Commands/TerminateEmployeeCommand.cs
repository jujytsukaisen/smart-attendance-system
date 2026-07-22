using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class TerminateEmployeeCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
    public DateTime TerminationDate { get; set; }
}

public class TerminateEmployeeCommandValidator : AbstractValidator<TerminateEmployeeCommand>
{
    public TerminateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.TerminationDate).NotEmpty();
    }
}

public class TerminateEmployeeCommandHandler : IRequestHandler<TerminateEmployeeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public TerminateEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(TerminateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        try
        {
            employee.Terminate(request.TerminationDate);

            return Result.Success();
        }
        catch (SmartAttendance.Domain.Exceptions.InvalidEmployeeException ex)
        {
            return Result.Failure(new[] { ex.Message });
        }
    }
}

