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

namespace SmartAttendance.Application.Features.Permissions.Queries;

public class GetPermissionByIdQuery : IRequest<Result<PermissionDto>>
{
    public Guid PermissionId { get; set; }
}

public class PermissionDto : IMapFrom<Permission>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Permission, PermissionDto>();
    }
}

public class GetPermissionByIdQueryValidator : AbstractValidator<GetPermissionByIdQuery>
{
    public GetPermissionByIdQueryValidator()
    {
        RuleFor(x => x.PermissionId).NotEmpty();
    }
}

public class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, Result<PermissionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPermissionByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PermissionDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PermissionId, cancellationToken);
            
        if (permission == null) return Result<PermissionDto>.Failure(new[] { "Permission not found." });
        
        var dto = _mapper.Map<PermissionDto>(permission);
        
        return Result<PermissionDto>.Success(dto);
    }
}

