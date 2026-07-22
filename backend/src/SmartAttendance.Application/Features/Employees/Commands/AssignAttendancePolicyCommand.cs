using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartAttendance.Application.Features.Employees.Commands;

public class AssignAttendancePolicyCommand : IRequest<Result>
{
    public Guid EmployeeId { get; set; }
    public Guid AttendancePolicyId { get; set; }
}

public class AssignAttendancePolicyCommandValidator : AbstractValidator<AssignAttendancePolicyCommand>
{
    public AssignAttendancePolicyCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
    }
}

public class AssignAttendancePolicyCommandHandler : IRequestHandler<AssignAttendancePolicyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignAttendancePolicyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignAttendancePolicyCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.EmployeeId }, cancellationToken);

        if (employee == null)
            return Result.Failure(new[] { "Employee not found." });

        // Add proper verification for Attendance Policy existence
        var policyExists = await _context.AttendancePolicies.AnyAsync(p => p.Id == request.AttendancePolicyId && p.CompanyId == employee.CompanyId, cancellationToken);
        if (!policyExists)
            return Result.Failure(new[] { "Attendance Policy not found or does not belong to the employee's company." });

        employee.AssignAttendancePolicy(request.AttendancePolicyId);

        return Result.Success();
    }
}

