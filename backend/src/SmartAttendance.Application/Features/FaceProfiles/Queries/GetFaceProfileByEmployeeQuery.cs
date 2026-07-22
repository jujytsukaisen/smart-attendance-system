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

namespace SmartAttendance.Application.Features.FaceProfiles.Queries;

public class GetFaceProfileByEmployeeQuery : IRequest<Result<FaceProfileDto>>
{
    public Guid EmployeeId { get; set; }
}

public class FaceProfileDto : IMapFrom<FaceProfile>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string FaceId { get; set; } = string.Empty;
    public string ReferenceImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<FaceProfile, FaceProfileDto>();
    }
}

public class GetFaceProfileByEmployeeQueryValidator : AbstractValidator<GetFaceProfileByEmployeeQuery>
{
    public GetFaceProfileByEmployeeQueryValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class GetFaceProfileByEmployeeQueryHandler : IRequestHandler<GetFaceProfileByEmployeeQuery, Result<FaceProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFaceProfileByEmployeeQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<FaceProfileDto>> Handle(GetFaceProfileByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.FaceProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId, cancellationToken);
            
        if (profile == null) return Result<FaceProfileDto>.Failure(new[] { "Face profile not found." });
        
        var dto = _mapper.Map<FaceProfileDto>(profile);
        
        return Result<FaceProfileDto>.Success(dto);
    }
}

