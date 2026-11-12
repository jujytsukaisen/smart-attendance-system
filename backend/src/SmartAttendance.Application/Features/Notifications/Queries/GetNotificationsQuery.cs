using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Notifications.Queries;

public class NotificationListDto : IMapFrom<Notification>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Notification, NotificationListDto>();
    }
}

public class GetNotificationsQuery : PaginationRequest, IRequest<Result<PagedResult<NotificationListDto>>>
{
    public string? Status { get; set; }
}

public class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PagedResult<NotificationListDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<NotificationListDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        // فلترة الإشعارات حسب المستخدم الحالي
        Guid.TryParse(_currentUserService.UserId, out var currentUserId);
        var query = _context.Notifications
            .Where(x => x.RecipientUserId == currentUserId)
            .AsNoTracking()
            .AsQueryable();

        // فلترة حسب الحالة (مقروء/غير مقروء)
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statusLower = request.Status.ToLower();
            if (statusLower == "read")
                query = query.Where(x => x.Status == SmartAttendance.Domain.Enums.NotificationStatus.Read);
            else if (statusLower == "unread")
                query = query.Where(x => x.Status != SmartAttendance.Domain.Enums.NotificationStatus.Read);
            else if (statusLower == "archived")
                query = query.Where(x => x.Status == SmartAttendance.Domain.Enums.NotificationStatus.Archived);
        }

        var count = await query.CountAsync(cancellationToken);
        var unreadCount = await _context.Notifications
            .Where(x => x.RecipientUserId == currentUserId && x.Status != SmartAttendance.Domain.Enums.NotificationStatus.Read)
            .CountAsync(cancellationToken);

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
            },
            UnreadCount = unreadCount
        };

        return Result<PagedResult<NotificationListDto>>.Success(result);
    }
}

