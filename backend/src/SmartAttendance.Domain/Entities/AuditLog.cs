using System;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string EntityName { get; private set; }
    public string EntityId { get; private set; }
    public AuditAction Action { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IPAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public new DateTime CreatedAt { get; private set; }
    public string? CorrelationId { get; private set; }

    // Completely immutable: No setters beyond initialization, no modifier methods
#pragma warning disable CS8618
    private AuditLog() { }
#pragma warning restore CS8618

    private AuditLog(
        Guid? userId,
        string entityName,
        string entityId,
        AuditAction action,
        string? oldValues,
        string? newValues,
        string? ipAddress,
        string? userAgent,
        string? correlationId)
    {
        UserId = userId;
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        OldValues = oldValues;
        NewValues = newValues;
        IPAddress = ipAddress;
        UserAgent = userAgent;
        CorrelationId = correlationId;
        
        CreatedAt = DateTime.UtcNow;
    }

    public static AuditLog Create(
        Guid? userId,
        string entityName,
        string entityId,
        AuditAction action,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new InvalidAuditLogException("Entity Name is required.");

        if (string.IsNullOrWhiteSpace(entityId))
            throw new InvalidAuditLogException("Entity ID is required.");

        if (!Enum.IsDefined(typeof(AuditAction), action))
            throw new InvalidAuditLogException("A valid Audit Action is required.");

        return new AuditLog(
            userId,
            entityName,
            entityId,
            action,
            oldValues,
            newValues,
            ipAddress,
            userAgent,
            correlationId);
    }
}
