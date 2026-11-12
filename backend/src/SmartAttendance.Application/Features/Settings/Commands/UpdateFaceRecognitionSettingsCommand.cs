using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Settings.Commands;

public class UpdateFaceRecognitionSettingsCommand : IRequest<Result>
{
    public double RecognitionThreshold { get; set; }
    public double MinimumFaceQuality { get; set; }
    public bool LivenessDetectionEnabled { get; set; }
    public bool MaskDetectionEnabled { get; set; }
    public bool SpoofDetectionEnabled { get; set; }
}

public class UpdateFaceRecognitionSettingsCommandValidator : AbstractValidator<UpdateFaceRecognitionSettingsCommand>
{
    public UpdateFaceRecognitionSettingsCommandValidator()
    {
        RuleFor(x => x.RecognitionThreshold).InclusiveBetween(0, 100);
        RuleFor(x => x.MinimumFaceQuality).InclusiveBetween(0, 100);
    }
}

public class UpdateFaceRecognitionSettingsCommandHandler : IRequestHandler<UpdateFaceRecognitionSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateFaceRecognitionSettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateFaceRecognitionSettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty) return Result.Failure(new[] { "Unable to determine company context." });

        await SettingsHelper.UpsertAsync(_context, companyId, "FaceRecognition", new Dictionary<string, string>
        {
            { "RecognitionThreshold", request.RecognitionThreshold.ToString() },
            { "MinimumFaceQuality", request.MinimumFaceQuality.ToString() },
            { "LivenessDetectionEnabled", request.LivenessDetectionEnabled.ToString() },
            { "MaskDetectionEnabled", request.MaskDetectionEnabled.ToString() },
            { "SpoofDetectionEnabled", request.SpoofDetectionEnabled.ToString() },
        }, cancellationToken);

        return Result.Success();
    }
}
