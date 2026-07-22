using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Notifications.Queries;

public class GetUnreadNotificationsQuery : PaginationRequest, IRequest<Result<PagedResult<NotificationListDto>>>
{
    public Guid UserId { get; set; }
}

public class GetUnreadNotificationsQueryValidator : AbstractValidator<GetUnreadNotificationsQuery>
{
    public GetUnreadNotificationsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetUnreadNotificationsQueryHandler : IRequestHandler<GetUnreadNotificationsQuery, Result<PagedResult<NotificationListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUnreadNotificationsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<NotificationListDto>>> Handle(GetUnreadNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications.Where(x => x.RecipientUserId == request.UserId && x.Status != SmartAttendance.Domain.Enums.NotificationStatus.Read).AsNoTracking().AsQueryable();

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

