using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

using SmartAttendance.Application.Features.AttendancePolicies.DTOs;

namespace SmartAttendance.Application.Features.AttendancePolicies.Queries;

public class GetAttendancePolicyByIdQuery : IRequest<Result<AttendancePolicyDetailsDto>>
{
    public Guid AttendancePolicyId { get; set; }
}



public class GetAttendancePolicyByIdQueryValidator : AbstractValidator<GetAttendancePolicyByIdQuery>
{
    public GetAttendancePolicyByIdQueryValidator()
    {
        RuleFor(x => x.AttendancePolicyId).NotEmpty();
    }
}

public class GetAttendancePolicyByIdQueryHandler : IRequestHandler<GetAttendancePolicyByIdQuery, Result<AttendancePolicyDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAttendancePolicyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<AttendancePolicyDetailsDto>> Handle(GetAttendancePolicyByIdQuery request, CancellationToken cancellationToken)
    {
        var policyEntity = await _context.AttendancePolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.AttendancePolicyId, cancellationToken);
            
        if (policyEntity == null)
            return Result<AttendancePolicyDetailsDto>.Failure(new[] { "Attendance policy not found." });

        var policy = _mapper.Map<AttendancePolicyDetailsDto>(policyEntity);

        return Result<AttendancePolicyDetailsDto>.Success(policy);
    }
}
