using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.FaceProfiles.Commands;

public class RegisterFaceCommand : IRequest<Result<Guid>>
{
    public Guid EmployeeId { get; set; }
    public byte[] FaceImage { get; set; } = Array.Empty<byte>();
    public string ImageContentType { get; set; } = string.Empty;
}

public class RegisterFaceCommandValidator : AbstractValidator<RegisterFaceCommand>
{
    public RegisterFaceCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.FaceImage).NotEmpty();
        RuleFor(x => x.ImageContentType).NotEmpty().MaximumLength(50);
    }
}

public class RegisterFaceCommandHandler : IRequestHandler<RegisterFaceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly IFaceRecognitionService _faceRecognitionService;

    public RegisterFaceCommandHandler(IApplicationDbContext context, IFaceRecognitionService faceRecognitionService)
    {
        _context = context;
        _faceRecognitionService = faceRecognitionService;
    }

    public async Task<Result<Guid>> Handle(RegisterFaceCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
        if (employee == null) return Result<Guid>.Failure(new[] { "Employee not found." });
        
        var profile = SmartAttendance.Domain.Entities.FaceProfile.Create(employee.CompanyId, request.EmployeeId);
        
        // Simulating external face profile ID registration
        var externalFaceProfileId = Guid.NewGuid().ToString(); 
        profile.Register(externalFaceProfileId);
        
        await _context.FaceProfiles.AddAsync(profile, cancellationToken);

        return Result<Guid>.Success(profile.Id);
    }
}

