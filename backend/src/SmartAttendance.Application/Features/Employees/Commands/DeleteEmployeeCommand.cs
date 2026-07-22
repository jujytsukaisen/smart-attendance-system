using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class DeleteEmployeeCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
}

public class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteEmployeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        employee.Delete();

        return Result.Success();
    }
}



