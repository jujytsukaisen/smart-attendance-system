using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Queries;

public class SearchNotificationsQuery : PaginationRequest, IRequest<Result<PagedResult<NotificationListDto>>>
{
    public string SearchTerm { get; set; } = string.Empty;
}

public class SearchNotificationsQueryValidator : AbstractValidator<SearchNotificationsQuery>
{
    public SearchNotificationsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class SearchNotificationsQueryHandler : IRequestHandler<SearchNotificationsQuery, Result<PagedResult<NotificationListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchNotificationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<NotificationListDto>>> Handle(SearchNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => x.Title.Contains(request.SearchTerm) || x.Message.Contains(request.SearchTerm));
        }

        var count = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<System.Collections.Generic.List<NotificationListDto>>(notifications);
        
        var result = new PagedResult<NotificationListDto>
        {
            Items = dtos,
            Pagination = new PaginationResponse 
            {
                TotalCount = count,
                PageNumber = request.PageNumber,
                TotalPages = (int)System.Math.Ceiling(count / (double)request.PageSize),
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < (int)System.Math.Ceiling(count / (double)request.PageSize)
            }
        };

        return Result<PagedResult<NotificationListDto>>.Success(result);
    }
}

