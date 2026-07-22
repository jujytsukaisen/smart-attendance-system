using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.FaceProfiles.Commands;

public class UpdateFaceProfileCommand : IRequest<Result>
{
    public Guid FaceProfileId { get; set; }
    public byte[] NewFaceImage { get; set; } = Array.Empty<byte>();
    public string ImageContentType { get; set; } = string.Empty;
}

public class UpdateFaceProfileCommandValidator : AbstractValidator<UpdateFaceProfileCommand>
{
    public UpdateFaceProfileCommandValidator()
    {
        RuleFor(x => x.FaceProfileId).NotEmpty();
        RuleFor(x => x.NewFaceImage).NotEmpty();
        RuleFor(x => x.ImageContentType).NotEmpty().MaximumLength(50);
    }
}

public class UpdateFaceProfileCommandHandler : IRequestHandler<UpdateFaceProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IFaceRecognitionService _faceRecognitionService;

    public UpdateFaceProfileCommandHandler(IApplicationDbContext context, IFaceRecognitionService faceRecognitionService)
    {
        _context = context;
        _faceRecognitionService = faceRecognitionService;
    }

    public async Task<Result> Handle(UpdateFaceProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.FaceProfiles.FirstOrDefaultAsync(x => x.Id == request.FaceProfileId, cancellationToken);
        if (profile == null) return Result.Failure(new[] { "Face profile not found." });
        
        var externalFaceProfileId = Guid.NewGuid().ToString();
        profile.UpdateProfile(externalFaceProfileId);
        
        _context.FaceProfiles.Update(profile);

        return Result.Success();
    }
}



