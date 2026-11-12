using FluentValidation;
using SmartAttendance.Application.Features.AttendanceDevices.Commands;
using System.Text.RegularExpressions;

namespace SmartAttendance.Application.Features.AttendanceDevices.Validators;

public class CreateAttendanceDeviceCommandValidator : AbstractValidator<CreateAttendanceDeviceCommand>
{
    public CreateAttendanceDeviceCommandValidator()
    {
        RuleFor(v => v.CompanyId)
            .NotEmpty().WithMessage("CompanyId is required.");

        RuleFor(v => v.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(v => v.Code)
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .NotEmpty().WithMessage("Code is required.");

        RuleFor(v => v.SerialNumber)
            .MaximumLength(200).WithMessage("SerialNumber must not exceed 200 characters.");

        RuleFor(v => v.DeviceType)
            .IsInEnum().WithMessage("DeviceType is invalid.");

        RuleFor(v => v.OperatingSystem)
            .MaximumLength(100).WithMessage("OperatingSystem must not exceed 100 characters.");

        RuleFor(v => v.ApplicationVersion)
            .MaximumLength(50).WithMessage("ApplicationVersion must not exceed 50 characters.");

        RuleFor(v => v.Latitude)
            .InclusiveBetween(-90, 90).When(v => v.Latitude.HasValue).WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(v => v.Longitude)
            .InclusiveBetween(-180, 180).When(v => v.Longitude.HasValue).WithMessage("Longitude must be between -180 and 180 degrees.");

        RuleFor(v => v.AllowedRadiusMeters)
            .GreaterThanOrEqualTo(0).When(v => v.AllowedRadiusMeters.HasValue).WithMessage("Radius cannot be negative.");

        RuleFor(v => v.PublicKey)
            .MaximumLength(4000).WithMessage("PublicKey must not exceed 4000 characters.");

        RuleFor(v => v.DeviceFingerprint)
            .MaximumLength(500).WithMessage("DeviceFingerprint must not exceed 500 characters.");

        RuleFor(v => v.Port)
            .InclusiveBetween(1, 65535).When(v => v.Port.HasValue).WithMessage("Port must be between 1 and 65535.");

        RuleFor(v => v.IpAddress)
            .MaximumLength(45).WithMessage("IpAddress must not exceed 45 characters.");

        RuleFor(v => v.CommunicationProtocol)
            .IsInEnum().WithMessage("CommunicationProtocol is invalid.");
    }
}
