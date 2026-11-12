using System;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.Exceptions;

namespace SmartAttendance.Domain.Entities;

public class FaceProfile : BaseTenantEntity
{
    public Guid EmployeeId { get; private set; }
    public string? FaceProfileId { get; private set; }

    // ⚠️ حقل جديد: بصمة الوجه (128 رقم عشري) مخزّنة كـ JSON، تُستخرج من face-api.js في المتصفح
    public string? FaceEncoding { get; private set; }

    public RegistrationStatus RegistrationStatus { get; private set; }
    public DateTimeOffset? RegisteredAt { get; private set; }
    public DateTimeOffset? LastUpdatedAt { get; private set; }
    public DateTimeOffset? LastVerificationAt { get; private set; }
    public bool IsActive { get; private set; }

#pragma warning disable CS8618
    private FaceProfile() { }
#pragma warning restore CS8618

    private FaceProfile(Guid companyId, Guid employeeId)
    {
        CompanyId = companyId;
        EmployeeId = employeeId;
        RegistrationStatus = RegistrationStatus.Pending;
        IsActive = false;
    }

    public static FaceProfile Create(Guid companyId, Guid employeeId)
    {
        if (companyId == Guid.Empty)
            throw new InvalidFaceProfileException("Company ID cannot be empty.");
        if (employeeId == Guid.Empty)
            throw new InvalidFaceProfileException("Employee ID cannot be empty.");

        return new FaceProfile(companyId, employeeId);
    }

    public void Register(string faceProfileId)
    {
        if (string.IsNullOrWhiteSpace(faceProfileId))
            throw new InvalidFaceProfileException("Face profile ID is required.");

        if (RegistrationStatus == RegistrationStatus.Registered)
            throw new InvalidFaceProfileException("Face profile is already registered.");

        FaceProfileId = faceProfileId;
        RegistrationStatus = RegistrationStatus.Registered;
        RegisteredAt = DateTimeOffset.UtcNow;
        LastUpdatedAt = DateTimeOffset.UtcNow;

        Activate();
    }

    // ⚠️ جديد: تخزين بصمة الوجه (JSON array من 128 رقم فلوت)
    public void SetEncoding(string encodingJson)
    {
        if (string.IsNullOrWhiteSpace(encodingJson))
            throw new InvalidFaceProfileException("Face encoding cannot be empty.");

        FaceEncoding = encodingJson;
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateProfile(string faceProfileId)
    {
        if (RegistrationStatus != RegistrationStatus.Registered)
            throw new InvalidFaceProfileException("Cannot update an unregistered face profile.");

        if (string.IsNullOrWhiteSpace(faceProfileId))
            throw new InvalidFaceProfileException("Face profile ID cannot be empty.");

        FaceProfileId = faceProfileId;
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordVerification()
    {
        if (RegistrationStatus != RegistrationStatus.Registered || string.IsNullOrWhiteSpace(FaceProfileId))
            throw new InvalidFaceProfileException("Cannot verify attendance using a face profile that is not registered.");

        if (!IsActive)
            throw new InvalidFaceProfileException("Inactive face profiles cannot be used for attendance verification.");

        LastVerificationAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (IsActive) return;

        if (RegistrationStatus != RegistrationStatus.Registered)
            throw new InvalidFaceProfileException("Cannot activate an unregistered face profile.");

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }
}