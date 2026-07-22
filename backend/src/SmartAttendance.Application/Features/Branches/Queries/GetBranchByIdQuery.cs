using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Application.Features.Branches.DTOs;

namespace SmartAttendance.Application.Features.Branches.Queries;

public class GetBranchByIdQuery : IRequest<Result<BranchDetailsDto>>
{
    public Guid BranchId { get; set; }
}

public class GetBranchByIdQueryValidator : AbstractValidator<GetBranchByIdQuery>
{
    public GetBranchByIdQueryValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, Result<BranchDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBranchByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<BranchDetailsDto>> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _context.Branches
            .AsNoTracking()
            .Where(b => b.Id == request.BranchId)
            .ProjectTo<BranchDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (dto == null)
        {
            return Result<BranchDetailsDto>.Failure(new[] { "Branch not found." });
        }

        return Result<BranchDetailsDto>.Success(dto);
    }
}
