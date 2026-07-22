using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Attendancess.Commands;

public class VerifyGPSCommand : IRequest<Result>
{
    public Guid AttendanceId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class VerifyGPSCommandValidator : AbstractValidator<VerifyGPSCommand>
{
    public VerifyGPSCommandValidator()
    {
        RuleFor(x => x.AttendanceId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90.0, 90.0);
        RuleFor(x => x.Longitude).InclusiveBetween(-180.0, 180.0);
    }
}

public class VerifyGPSCommandHandler : IRequestHandler<VerifyGPSCommand, Result>
{
    private readonly IGeoLocationService _geoLocationService;
    private readonly IApplicationDbContext _context;

    public VerifyGPSCommandHandler(IGeoLocationService geoLocationService, IApplicationDbContext context)
    {
        _geoLocationService = geoLocationService;
        _context = context;
    }

    public async Task<Result> Handle(VerifyGPSCommand request, CancellationToken cancellationToken)
    {
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(x => x.Id == request.AttendanceId, cancellationToken);
            
        if (attendance == null)
            return Result.Failure(new[] { "Attendance not found." });

        // Validate location against a placeholder target (e.g., office location)
        var locationResult = await _geoLocationService.ValidateLocationAsync(request.Latitude, request.Longitude, request.Latitude, request.Longitude, 100);
        if (!locationResult.Succeeded || !locationResult.Data)
        {
            return Result.Failure(locationResult.Errors);
        }
            
        attendance.MarkGPSVerified(true);
        
        _context.Attendances.Update(attendance);

        return Result.Success();
    }
}

