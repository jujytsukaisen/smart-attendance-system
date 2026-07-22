using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.FaceProfiles.Commands;

public class DeactivateFaceProfileCommand : IRequest<Result>
{
    public Guid FaceProfileId { get; set; }
}

public class DeactivateFaceProfileCommandValidator : AbstractValidator<DeactivateFaceProfileCommand>
{
    public DeactivateFaceProfileCommandValidator()
    {
        RuleFor(x => x.FaceProfileId).NotEmpty();
    }
}

public class DeactivateFaceProfileCommandHandler : IRequestHandler<DeactivateFaceProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateFaceProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateFaceProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.FaceProfiles.FirstOrDefaultAsync(x => x.Id == request.FaceProfileId, cancellationToken);
        if (profile == null) return Result.Failure(new[] { "Face profile not found." });
        
        profile.Deactivate();
        _context.FaceProfiles.Update(profile);

        return Result.Success();
    }
}

