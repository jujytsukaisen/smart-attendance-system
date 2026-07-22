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

public class NotificationDto : IMapFrom<Notification>
{
    public Guid Id { get; set; }
    public Guid RecipientUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Notification, NotificationDto>();
    }
}

public class GetNotificationsByUserQuery : PaginationRequest, IRequest<Result<PagedResult<NotificationDto>>>
{
    public Guid UserId { get; set; }
}

public class GetNotificationsByUserQueryValidator : AbstractValidator<GetNotificationsByUserQuery>
{
    public GetNotificationsByUserQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}

public class GetNotificationsByUserQueryHandler : IRequestHandler<GetNotificationsByUserQuery, Result<PagedResult<NotificationDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetNotificationsByUserQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<NotificationDto>>> Handle(GetNotificationsByUserQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications.Where(x => x.RecipientUserId == request.UserId).AsNoTracking().AsQueryable();

        var count = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<System.Collections.Generic.List<NotificationDto>>(notifications);
        
        var result = new PagedResult<NotificationDto>
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

        return Result<PagedResult<NotificationDto>>.Success(result);
    }
}

