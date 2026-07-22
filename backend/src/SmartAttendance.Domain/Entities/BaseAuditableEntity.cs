namespace SmartAttendance.Domain.Entities;

public abstract class BaseAuditableEntity : BaseEntity
{
    public string? CreatedBy { get; internal set; }
    public string? UpdatedBy { get; internal set; }
    public string? DeletedBy { get; internal set; }
}
