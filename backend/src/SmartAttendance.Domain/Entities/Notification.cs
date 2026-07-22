using System;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class Notification : BaseTenantEntity
{
    public Guid RecipientUserId { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public NotificationStatus Status { get; private set; }

    public new DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    public User User { get; private set; } = null!;

    // Private constructor for EF Core materialization (persistence agnostic)
#pragma warning disable CS8618
    private Notification() { }
#pragma warning restore CS8618

    private Notification(
        Guid companyId,
        Guid recipientUserId,
        string title,
        string message,
        NotificationType notificationType,
        NotificationPriority priority)
    {
        CompanyId = companyId;
        RecipientUserId = recipientUserId;
        Title = title;
        Message = message;
        NotificationType = notificationType;
        Priority = priority;
        
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static Notification Create(
        Guid companyId,
        Guid recipientUserId,
        string title,
        string message,
        NotificationType notificationType,
        NotificationPriority priority)
    {
        if (companyId == Guid.Empty)
            throw new InvalidNotificationException("Company ID is required.");
        if (recipientUserId == Guid.Empty)
            throw new InvalidNotificationException("Recipient User ID is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidNotificationException("Title is required.");
        if (string.IsNullOrWhiteSpace(message))
            throw new InvalidNotificationException("Message is required.");

        return new Notification(
            companyId,
            recipientUserId,
            title,
            message,
            notificationType,
            priority);
    }

    public void MarkAsSent()
    {
        if (Status == NotificationStatus.Sent || Status == NotificationStatus.Read || Status == NotificationStatus.Archived)
            return;

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Archived)
            throw new InvalidNotificationException("Cannot read an archived notification.");

        if (Status == NotificationStatus.Read)
            return;

        Status = NotificationStatus.Read;
        ReadAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status == NotificationStatus.Pending)
            throw new InvalidNotificationException("Cannot mark unread notification as archived before sending.");

        if (Status == NotificationStatus.Archived)
            return;

        Status = NotificationStatus.Archived;
    }
}
