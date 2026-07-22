using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.FaceProfiles.Commands;

public class VerifyFaceCommand : IRequest<Result<bool>>
{
    public Guid EmployeeId { get; set; }
    public byte[] CapturedImage { get; set; } = Array.Empty<byte>();
    public string ImageContentType { get; set; } = string.Empty;
}

public class VerifyFaceCommandValidator : AbstractValidator<VerifyFaceCommand>
{
    public VerifyFaceCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.CapturedImage).NotEmpty();
        RuleFor(x => x.ImageContentType).NotEmpty().MaximumLength(50);
    }
}

public class VerifyFaceCommandHandler : IRequestHandler<VerifyFaceCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFaceRecognitionService _faceRecognitionService;

    public VerifyFaceCommandHandler(IApplicationDbContext context, IFaceRecognitionService faceRecognitionService)
    {
        _context = context;
        _faceRecognitionService = faceRecognitionService;
    }

    public async Task<Result<bool>> Handle(VerifyFaceCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.FaceProfiles.FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId, cancellationToken);
        if (profile == null) return Result<bool>.Failure(new[] { "Face profile not found." });
        
        var isVerifiedResult = await _faceRecognitionService.VerifyFaceAsync("photoRef", request.EmployeeId.ToString());
        if (!isVerifiedResult.Succeeded || !isVerifiedResult.Data) return Result<bool>.Failure(isVerifiedResult.Errors);
        
        profile.RecordVerification();
        _context.FaceProfiles.Update(profile);

        return Result<bool>.Success(true);
    }
}

