using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Breaks.Queries;

using SmartAttendance.Application.Features.Breaks.DTOs;

public class GetBreaksByShiftQuery : PaginationRequest, IRequest<Result<PagedResult<BreakDto>>>
{
    public Guid ShiftId { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public class GetBreaksByShiftQueryValidator : AbstractValidator<GetBreaksByShiftQuery>
{
    public GetBreaksByShiftQueryValidator()
    {
        RuleFor(x => x.ShiftId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetBreaksByShiftQueryHandler : IRequestHandler<GetBreaksByShiftQuery, Result<PagedResult<BreakDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBreaksByShiftQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<BreakDto>>> Handle(GetBreaksByShiftQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Breaks
            .AsNoTracking()
            .Where(b => b.ShiftId == request.ShiftId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var cleanSearchTerm = request.SearchTerm.Trim();
            query = query.Where(x => x.Name.Contains(cleanSearchTerm));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "starttime" => request.SortDesc ? query.OrderByDescending(x => x.StartTime) : query.OrderBy(x => x.StartTime),
            _ => request.SortDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<BreakDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<BreakDto>
        {
            Items = items,
            Pagination = new PaginationResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                HasNextPage = request.PageNumber * request.PageSize < totalCount,
                HasPreviousPage = request.PageNumber > 1
            }
        };

        return Result<PagedResult<BreakDto>>.Success(result);
    }
}
