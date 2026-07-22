using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Leaves.Queries;

public class LeaveBalanceDto
{
    public Guid EmployeeId { get; set; }
    public int AnnualLeaveTotal { get; set; }
    public int AnnualLeaveUsed { get; set; }
    public int SickLeaveTotal { get; set; }
    public int SickLeaveUsed { get; set; }
}

public class GetLeaveBalanceQuery : IRequest<Result<LeaveBalanceDto>>
{
    public Guid EmployeeId { get; set; }
}

public class GetLeaveBalanceQueryValidator : AbstractValidator<GetLeaveBalanceQuery>
{
    public GetLeaveBalanceQueryValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class GetLeaveBalanceQueryHandler : IRequestHandler<GetLeaveBalanceQuery, Result<LeaveBalanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveBalanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LeaveBalanceDto>> Handle(GetLeaveBalanceQuery request, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var startOfYear = new DateTime(year, 1, 1);
        var endOfYear = new DateTime(year + 1, 1, 1);

        var leaves = await _context.Leaves
            .Where(x => x.EmployeeId == request.EmployeeId && x.StartDate >= startOfYear && x.StartDate < endOfYear)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var annualLeaveUsed = leaves.Where(x => x.LeaveType == SmartAttendance.Domain.Enums.LeaveType.Annual && x.LeaveStatus == SmartAttendance.Domain.Enums.LeaveStatus.Approved).Sum(x => (int)x.TotalDays);
        var sickLeaveUsed = leaves.Where(x => x.LeaveType == SmartAttendance.Domain.Enums.LeaveType.Sick && x.LeaveStatus == SmartAttendance.Domain.Enums.LeaveStatus.Approved).Sum(x => (int)x.TotalDays);

        var dto = new LeaveBalanceDto 
        { 
            EmployeeId = request.EmployeeId,
            AnnualLeaveTotal = 20, // Default for now
            AnnualLeaveUsed = annualLeaveUsed,
            SickLeaveTotal = 10,
            SickLeaveUsed = sickLeaveUsed
        };
        return Result<LeaveBalanceDto>.Success(dto);
    }
}

