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

public class GetFaceProfileByIdQuery : IRequest<Result<FaceProfileDetailsDto>>
{
    public Guid FaceProfileId { get; set; }
}

public class FaceProfileDetailsDto : IMapFrom<FaceProfile>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string FaceId { get; set; } = string.Empty;
    public string ReferenceImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastVerifiedAt { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<FaceProfile, FaceProfileDetailsDto>();
    }
}

public class GetFaceProfileByIdQueryValidator : AbstractValidator<GetFaceProfileByIdQuery>
{
    public GetFaceProfileByIdQueryValidator()
    {
        RuleFor(x => x.FaceProfileId).NotEmpty();
    }
}

public class GetFaceProfileByIdQueryHandler : IRequestHandler<GetFaceProfileByIdQuery, Result<FaceProfileDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFaceProfileByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<FaceProfileDetailsDto>> Handle(GetFaceProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.FaceProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.FaceProfileId, cancellationToken);
            
        if (profile == null) return Result<FaceProfileDetailsDto>.Failure(new[] { "Face profile not found." });
        
        var dto = _mapper.Map<FaceProfileDetailsDto>(profile);
        
        return Result<FaceProfileDetailsDto>.Success(dto);
    }
}

