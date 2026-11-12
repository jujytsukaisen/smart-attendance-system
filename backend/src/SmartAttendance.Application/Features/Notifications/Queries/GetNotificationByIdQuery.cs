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

namespace SmartAttendance.Application.Features.Notifications.Queries;

public class NotificationDetailsDto : IMapFrom<Notification>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid RecipientUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Notification, NotificationDetailsDto>();
    }
}

public class GetNotificationByIdQuery : IRequest<Result<NotificationDetailsDto>>
{
    public Guid NotificationId { get; set; }
}

public class GetNotificationByIdQueryValidator : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdQueryValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDetailsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationByIdQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<NotificationDetailsDto>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        Guid.TryParse(_currentUserService.UserId, out var currentUserId);

        var notification = await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.NotificationId, cancellationToken);
            
        if (notification == null || notification.RecipientUserId != currentUserId)
            return Result<NotificationDetailsDto>.Failure(new[] { "Notification not found." });
        
        var dto = _mapper.Map<NotificationDetailsDto>(notification);
        
        return Result<NotificationDetailsDto>.Success(dto);
    }
}

