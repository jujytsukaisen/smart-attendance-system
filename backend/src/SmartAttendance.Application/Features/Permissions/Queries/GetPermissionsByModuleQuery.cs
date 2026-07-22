using FluentValidation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Permissions.Queries;

public class GetPermissionsByModuleQuery : IRequest<Result<List<PermissionDto>>>
{
    public string Module { get; set; } = string.Empty;
}

public class GetPermissionsByModuleQueryValidator : AbstractValidator<GetPermissionsByModuleQuery>
{
    public GetPermissionsByModuleQueryValidator()
    {
        RuleFor(x => x.Module).NotEmpty().MaximumLength(50);
    }
}

public class GetPermissionsByModuleQueryHandler : IRequestHandler<GetPermissionsByModuleQuery, Result<List<PermissionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPermissionsByModuleQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<PermissionDto>>> Handle(GetPermissionsByModuleQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _context.Permissions
            .Where(x => x.Module == request.Module)
            .OrderBy(x => x.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<List<PermissionDto>>(permissions);
        
        return Result<List<PermissionDto>>.Success(dtos);
    }
}

