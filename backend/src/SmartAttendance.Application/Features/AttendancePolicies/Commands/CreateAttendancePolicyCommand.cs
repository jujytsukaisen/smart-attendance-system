using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.AttendancePolicies.Commands;

public class CreateAttendancePolicyCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Attendance Rules
    public AttendanceMode AttendanceMode { get; set; }
    public bool RequireFaceRecognition { get; set; }
    public bool RequireGPS { get; set; }
    public bool RequireApprovedDevice { get; set; }
    public bool RequireCompanyNetwork { get; set; }
    public bool AllowMobileCheckIn { get; set; }
    public bool AllowDesktopCheckIn { get; set; }
    public bool AllowWebCheckIn { get; set; }
    public bool AllowOfflineAttendance { get; set; }
    public double AllowedRadiusMeters { get; set; }
    public bool AllowMultipleCheckIn { get; set; }
    public bool AllowManualAttendance { get; set; }
    public bool RequireManagerApproval { get; set; }
    public double MaximumDistanceMeters { get; set; }

    public int LateToleranceMinutes { get; set; }
    public int EarlyLeaveToleranceMinutes { get; set; }
    public int GracePeriodMinutes { get; set; }
    public int OvertimeThresholdMinutes { get; set; }
    public int MinimumOvertimeMinutes { get; set; }
    public int MaximumOvertimeMinutes { get; set; }
    
    public bool AutoCheckoutEnabled { get; set; }
    public TimeOnly? AutoCheckoutTime { get; set; }
    
    public bool MultipleShiftsSupport { get; set; }
    
    public RoundTimeRule RoundTimeRule { get; set; }
    public MissingPunchPolicy MissingPunchPolicy { get; private set; }
    public WeekendPolicy WeekendPolicy { get; private set; }
    public HolidayPolicy HolidayPolicy { get; private set; }
}

public class CreateAttendancePolicyCommandValidator : AbstractValidator<CreateAttendancePolicyCommand>
{
    public CreateAttendancePolicyCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(250);
        
        RuleFor(x => x.AllowedRadiusMeters).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaximumDistanceMeters).GreaterThanOrEqualTo(0);

        RuleFor(x => x.LateToleranceMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EarlyLeaveToleranceMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GracePeriodMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OvertimeThresholdMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumOvertimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaximumOvertimeMinutes).GreaterThanOrEqualTo(x => x.MinimumOvertimeMinutes)
            .WithMessage("Maximum overtime cannot be less than minimum overtime.");
        
        RuleFor(x => x).Custom((command, context) =>
        {
            if (command.AttendanceMode == AttendanceMode.Field && !command.RequireGPS)
                context.AddFailure("RequireGPS", "Field mode requires GPS.");
                
            if (command.AttendanceMode == AttendanceMode.Office && !command.RequireFaceRecognition)
                context.AddFailure("RequireFaceRecognition", "Office mode requires Face Recognition.");
                
            if (command.AttendanceMode == AttendanceMode.Remote && command.RequireCompanyNetwork)
                context.AddFailure("RequireCompanyNetwork", "Remote mode cannot require Company Network.");
        });
    }
}

public class CreateAttendancePolicyCommandHandler : IRequestHandler<CreateAttendancePolicyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateAttendancePolicyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateAttendancePolicyCommand request, CancellationToken cancellationToken)
    {
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists) return Result<Guid>.Failure(new[] { "Company not found." });

        var duplicateCode = await _context.AttendancePolicies.AnyAsync(p => p.CompanyId == request.CompanyId && p.Code == request.Code, cancellationToken);
        if (duplicateCode) return Result<Guid>.Failure(new[] { "Attendance policy code already exists in the company." });

        var duplicateName = await _context.AttendancePolicies.AnyAsync(p => p.CompanyId == request.CompanyId && p.Name == request.Name, cancellationToken);
        if (duplicateName) return Result<Guid>.Failure(new[] { "Attendance policy name already exists in the company." });

        var policy = AttendancePolicy.Create(
            request.CompanyId,
            request.Name,
            request.Code,
            request.Description,
            request.AttendanceMode,
            request.RequireFaceRecognition,
            request.RequireGPS,
            request.RequireApprovedDevice,
            request.RequireCompanyNetwork,
            request.AllowMobileCheckIn,
            request.AllowDesktopCheckIn,
            request.AllowWebCheckIn,
            request.AllowOfflineAttendance,
            request.AllowedRadiusMeters,
            request.AllowMultipleCheckIn,
            request.AllowManualAttendance,
            request.RequireManagerApproval,
            request.MaximumDistanceMeters,
            request.LateToleranceMinutes,
            request.EarlyLeaveToleranceMinutes,
            request.GracePeriodMinutes,
            request.OvertimeThresholdMinutes,
            request.MinimumOvertimeMinutes,
            request.MaximumOvertimeMinutes,
            request.AutoCheckoutEnabled,
            request.AutoCheckoutTime,
            request.MultipleShiftsSupport,
            request.RoundTimeRule,
            request.MissingPunchPolicy,
            request.WeekendPolicy,
            request.HolidayPolicy);

        _context.AttendancePolicies.Add(policy);

        return Result<Guid>.Success(policy.Id);
    }
}

