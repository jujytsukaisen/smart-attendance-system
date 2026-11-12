using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Leaves.Queries;

public class LeaveListDto : IMapFrom<Leave>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNumber { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Leave, LeaveListDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.LeaveStatus.ToString()))
            .ForMember(d => d.LeaveType, opt => opt.MapFrom(s => s.LeaveType.ToString()))
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? $"{s.Employee.FirstName} {s.Employee.LastName}" : string.Empty))
            .ForMember(d => d.EmployeeNumber, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.EmployeeNumber : string.Empty));
    }
}

public class GetLeavesQuery : PaginationRequest, IRequest<Result<PagedResult<LeaveListDto>>>
{
}

public class GetLeavesQueryValidator : AbstractValidator<GetLeavesQuery>
{
    public GetLeavesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetLeavesQueryHandler : IRequestHandler<GetLeavesQuery, Result<PagedResult<LeaveListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetLeavesQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<LeaveListDto>>> Handle(GetLeavesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Leaves.AsNoTracking().Include(l => l.Employee).AsQueryable();

        if (Guid.TryParse(_currentUserService.TenantId, out var companyId))
        {
            query = query.Where(x => x.Employee.CompanyId == companyId);
        }

        if (Guid.TryParse(_currentUserService.UserId, out var currentUserId))
        {
            var userRoles = await _context.Users
                .Where(u => u.Id == currentUserId)
                .SelectMany(u => u.Roles.Select(r => r.Name))
                .ToListAsync(cancellationToken);

            bool isEmployeeOnly = userRoles.Count > 0 && userRoles.All(r => r == "موظف" || r == "Employee");
            if (isEmployeeOnly)
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == currentUserId, cancellationToken);
                if (employee != null)
                {
                    query = query.Where(x => x.EmployeeId == employee.Id);
                }
                else
                {
                    query = query.Where(x => false);
                }
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var leaves = await query
            .OrderByDescending(x => x.StartDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<System.Collections.Generic.List<LeaveListDto>>(leaves);
        
        var result = new PagedResult<LeaveListDto>
        {
            Items = dtos,
            Pagination = new PaginationResponse 
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < (int)System.Math.Ceiling(totalCount / (double)request.PageSize)
            }
        };
        
        return Result<PagedResult<LeaveListDto>>.Success(result);
    }
}
