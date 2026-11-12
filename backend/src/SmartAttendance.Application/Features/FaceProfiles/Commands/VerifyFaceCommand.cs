using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.FaceProfiles.Commands;

public class VerifyFaceCommand : IRequest<Result<FaceMatchResult>>
{
    public Guid EmployeeId { get; set; }
    public float[] SubmittedEncoding { get; set; } = Array.Empty<float>();
}

public class VerifyFaceCommandValidator : AbstractValidator<VerifyFaceCommand>
{
    public VerifyFaceCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.SubmittedEncoding)
            .NotEmpty()
            .Must(e => e.Length == 128)
            .WithMessage("Submitted face encoding must contain exactly 128 values.");
    }
}

public class VerifyFaceCommandHandler : IRequestHandler<VerifyFaceCommand, Result<FaceMatchResult>>
{
    private readonly IFaceRecognitionService _faceRecognitionService;

    public VerifyFaceCommandHandler(IFaceRecognitionService faceRecognitionService)
    {
        _faceRecognitionService = faceRecognitionService;
    }

    public async Task<Result<FaceMatchResult>> Handle(VerifyFaceCommand request, CancellationToken cancellationToken)
    {
        return await _faceRecognitionService.VerifyFaceAsync(request.EmployeeId, request.SubmittedEncoding);
    }
}