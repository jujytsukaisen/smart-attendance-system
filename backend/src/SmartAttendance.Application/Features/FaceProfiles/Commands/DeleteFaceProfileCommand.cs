using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.FaceProfiles.Commands;

public class DeleteFaceProfileCommand : IRequest<Result>
{
    public Guid FaceProfileId { get; set; }
}

public class DeleteFaceProfileCommandValidator : AbstractValidator<DeleteFaceProfileCommand>
{
    public DeleteFaceProfileCommandValidator()
    {
        RuleFor(x => x.FaceProfileId).NotEmpty();
    }
}

public class DeleteFaceProfileCommandHandler : IRequestHandler<DeleteFaceProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteFaceProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteFaceProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.FaceProfiles.FirstOrDefaultAsync(x => x.Id == request.FaceProfileId, cancellationToken);
        if (profile == null) return Result.Failure(new[] { "Face profile not found." });
        
        _context.FaceProfiles.Remove(profile);

        return Result.Success();
    }
}



