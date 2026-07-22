using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.AttendanceDevices.Commands;

public class UpdateAttendanceDeviceCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    
    public DeviceType DeviceType { get; set; }
    public string? OperatingSystem { get; set; }
    public string? ApplicationVersion { get; set; }
    
    public Guid? BranchId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double AllowedRadiusMeters { get; set; }
    
    public string? PublicKey { get; set; }
    public string? DeviceFingerprint { get; set; }
    public bool RequireDeviceApproval { get; set; }
    public bool RequireSecureConnection { get; set; }
    
    public bool SupportsFaceRecognition { get; set; }
    public bool SupportsFingerprint { get; set; }
    public bool SupportsGPS { get; set; }
    public bool SupportsOfflineAttendance { get; set; }
    public bool SupportsQRCode { get; set; }
    public bool SupportsNFC { get; set; }
    
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public CommunicationProtocol CommunicationProtocol { get; set; }
}

public class UpdateAttendanceDeviceCommandHandler : IRequestHandler<UpdateAttendanceDeviceCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateAttendanceDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateAttendanceDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.AttendanceDevices.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (device == null)
            return Result<Unit>.Failure(new[] { "Attendance device not found." });

        if (request.BranchId.HasValue)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId.Value, cancellationToken);
            if (branch == null || branch.CompanyId != device.CompanyId)
                return Result<Unit>.Failure(new[] { "Branch not found or does not belong to the company." });
        }

        device.Update(
            request.Name,
            request.Code,
            request.SerialNumber,
            request.DeviceType,
            request.OperatingSystem,
            request.ApplicationVersion,
            request.BranchId,
            request.Latitude,
            request.Longitude,
            request.AllowedRadiusMeters,
            request.PublicKey,
            request.DeviceFingerprint,
            request.RequireDeviceApproval,
            request.RequireSecureConnection,
            request.SupportsFaceRecognition,
            request.SupportsFingerprint,
            request.SupportsGPS,
            request.SupportsOfflineAttendance,
            request.SupportsQRCode,
            request.SupportsNFC,
            request.IpAddress,
            request.Port,
            request.CommunicationProtocol
        );

        return Result<Unit>.Success(Unit.Value);
    }
}
