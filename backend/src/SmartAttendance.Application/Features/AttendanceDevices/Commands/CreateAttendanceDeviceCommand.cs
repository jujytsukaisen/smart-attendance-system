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

public class CreateAttendanceDeviceCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    
    public DeviceType DeviceType { get; set; }
    public string? OperatingSystem { get; set; }
    public string? ApplicationVersion { get; set; }
    
    public Guid? BranchId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? AllowedRadiusMeters { get; set; }
    
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

public class CreateAttendanceDeviceCommandHandler : IRequestHandler<CreateAttendanceDeviceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateAttendanceDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateAttendanceDeviceCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (company == null)
            return Result<Guid>.Failure(new[] { "Company not found." });

        if (request.BranchId.HasValue)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == request.BranchId.Value, cancellationToken);
            if (branch == null || branch.CompanyId != request.CompanyId)
                return Result<Guid>.Failure(new[] { "Branch not found or does not belong to the company." });
        }

        var device = AttendanceDevice.Create(
            request.CompanyId,
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

        _context.AttendanceDevices.Add(device);

        // Intentionally omitting SaveChangesAsync, TransactionBehavior will handle it.
        return Result<Guid>.Success(device.Id);
    }
}
