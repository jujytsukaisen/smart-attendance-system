namespace SmartAttendance.Domain.Enums;

public enum MissingPunchPolicy
{
    Reject = 0,
    MarkAsAbsent = 1,
    RequireApproval = 2,
    DeductHalfDay = 3
}
