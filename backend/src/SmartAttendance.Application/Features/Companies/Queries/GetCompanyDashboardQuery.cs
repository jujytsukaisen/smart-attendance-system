using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Companies.Queries;

public class GetCompanyDashboardQuery : IRequest<Result<CompanyDashboardDto>>
{
    public Guid CompanyId { get; set; }
}

public class CompanyDashboardDto
{
    public int TotalEmployees { get; set; }
    public int ActiveBranches { get; set; }
    public int PendingLeaves { get; set; }
    public int PresentToday { get; set; }
    public int AbsentToday { get; set; }
}

public class GetCompanyDashboardQueryValidator : AbstractValidator<GetCompanyDashboardQuery>
{
    public GetCompanyDashboardQueryValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}

public class GetCompanyDashboardQueryHandler : IRequestHandler<GetCompanyDashboardQuery, Result<CompanyDashboardDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCompanyDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CompanyDashboardDto>> Handle(GetCompanyDashboardQuery request, CancellationToken cancellationToken)
    {
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
            return Result<CompanyDashboardDto>.Failure(new[] { "Company not found." });

        var totalEmployees = await _context.Employees
            .CountAsync(e => e.CompanyId == request.CompanyId && e.EmployeeStatus == SmartAttendance.Domain.Enums.EmployeeStatus.Active, cancellationToken);

        var activeBranches = await _context.Branches
            .CountAsync(b => b.CompanyId == request.CompanyId && b.IsActive, cancellationToken);

        var pendingLeaves = await _context.Leaves
            .CountAsync(l => l.Employee.CompanyId == request.CompanyId && l.LeaveStatus == SmartAttendance.Domain.Enums.LeaveStatus.Pending, cancellationToken);

        var today = System.DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var presentToday = await _context.Attendances
            .CountAsync(a => a.Employee.CompanyId == request.CompanyId && a.CheckInTime >= today && a.CheckInTime < tomorrow, cancellationToken);

        // Calculate absent today: active employees - present today
        var absentToday = totalEmployees - presentToday;
        if (absentToday < 0) absentToday = 0;

        var dto = new CompanyDashboardDto
        {
            TotalEmployees = totalEmployees,
            ActiveBranches = activeBranches,
            PendingLeaves = pendingLeaves,
            PresentToday = presentToday,
            AbsentToday = absentToday
        };

        return Result<CompanyDashboardDto>.Success(dto);
    }
}
