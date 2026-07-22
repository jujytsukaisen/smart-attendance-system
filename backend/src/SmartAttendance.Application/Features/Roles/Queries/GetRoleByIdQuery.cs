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

namespace SmartAttendance.Application.Features.Roles.Queries;

public class GetRoleByIdQuery : IRequest<Result<RoleDto>>
{
    public Guid RoleId { get; set; }
}

public class RoleDto : IMapFrom<Role>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Role, RoleDto>();
    }
}

public class GetRoleByIdQueryValidator : AbstractValidator<GetRoleByIdQuery>
{
    public GetRoleByIdQueryValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
    }
}

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRoleByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Include(x => x.Permissions)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.RoleId, cancellationToken);
            
        if (role == null) return Result<RoleDto>.Failure(new[] { "Role not found." });
        
        var dto = _mapper.Map<RoleDto>(role);
        
        return Result<RoleDto>.Success(dto);
    }
}

