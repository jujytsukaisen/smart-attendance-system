using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Leaves.Queries;

public class LeaveDetailsDto : IMapFrom<Leave>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNumber { get; set; }
    public Guid? ApproverId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ManagerComment { get; set; } = string.Empty;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Leave, LeaveDetailsDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.LeaveStatus.ToString()))
            .ForMember(d => d.LeaveType, opt => opt.MapFrom(s => s.LeaveType.ToString()))
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? $"{s.Employee.FirstName} {s.Employee.LastName}" : string.Empty))
            .ForMember(d => d.EmployeeNumber, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.EmployeeNumber : string.Empty));
    }
}

public class GetLeaveByIdQuery : IRequest<Result<LeaveDetailsDto>>
{
    public Guid LeaveId { get; set; }
}

public class GetLeaveByIdQueryValidator : AbstractValidator<GetLeaveByIdQuery>
{
    public GetLeaveByIdQueryValidator()
    {
        RuleFor(x => x.LeaveId).NotEmpty();
    }
}

public class GetLeaveByIdQueryHandler : IRequestHandler<GetLeaveByIdQuery, Result<LeaveDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetLeaveByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<LeaveDetailsDto>> Handle(GetLeaveByIdQuery request, CancellationToken cancellationToken)
    {
        var leave = await _context.Leaves
            .Include(x => x.Employee)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.LeaveId, cancellationToken);
            
        if (leave == null) return Result<LeaveDetailsDto>.Failure(new[] { "Leave not found." });
        
        var dto = _mapper.Map<LeaveDetailsDto>(leave);
        
        return Result<LeaveDetailsDto>.Success(dto);
    }
}

