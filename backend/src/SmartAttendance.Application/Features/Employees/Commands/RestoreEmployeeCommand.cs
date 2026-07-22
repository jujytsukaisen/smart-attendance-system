using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class RestoreEmployeeCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
}

public class RestoreEmployeeCommandValidator : AbstractValidator<RestoreEmployeeCommand>
{
    public RestoreEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class RestoreEmployeeCommandHandler : IRequestHandler<RestoreEmployeeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RestoreEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RestoreEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Must ignore query filters to find deleted employee
        var employee = await _context.Employees
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        employee.Restore();

        return Result.Success();
    }
}
