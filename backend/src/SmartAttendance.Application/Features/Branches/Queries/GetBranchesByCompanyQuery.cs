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

public class GetBranchesByCompanyQuery : IRequest<Result<PagedResult<BranchListDto>>>
{
    public Guid CompanyId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetBranchesByCompanyQueryValidator : AbstractValidator<GetBranchesByCompanyQuery>
{
    public GetBranchesByCompanyQueryValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetBranchesByCompanyQueryHandler : IRequestHandler<GetBranchesByCompanyQuery, Result<PagedResult<BranchListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBranchesByCompanyQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<BranchListDto>>> Handle(GetBranchesByCompanyQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Branches.AsNoTracking().Where(b => b.CompanyId == request.CompanyId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(b => b.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<BranchListDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<BranchListDto>
        {
            Items = items,
            Pagination = new PaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            }
        };

        return Result<PagedResult<BranchListDto>>.Success(result);
    }
}
