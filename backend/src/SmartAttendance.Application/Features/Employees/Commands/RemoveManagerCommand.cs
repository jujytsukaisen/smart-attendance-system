using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class RemoveManagerCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
}

public class RemoveManagerCommandValidator : AbstractValidator<RemoveManagerCommand>
{
    public RemoveManagerCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
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
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        employee.RemoveManager();

        return Result.Success();
    }
}

