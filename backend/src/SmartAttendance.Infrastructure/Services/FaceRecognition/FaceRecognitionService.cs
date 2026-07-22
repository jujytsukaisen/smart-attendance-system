using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Infrastructure.Options;
using SmartAttendance.Infrastructure.Services.FaceRecognition.Providers;

namespace SmartAttendance.Infrastructure.Services.FaceRecognition;

public class FaceRecognitionService : IFaceRecognitionService
{
    private readonly ILogger<FaceRecognitionService> _logger;
    private readonly FaceRecognitionOptions _options;
    private readonly IFaceRecognitionProvider _provider;

    public FaceRecognitionService(
        ILogger<FaceRecognitionService> logger, 
        IOptions<FaceRecognitionOptions> options,
        IFaceRecognitionProvider provider)
    {
        _logger = logger;
        _options = options.Value;
        _provider = provider;
    }

    public Task<Result<bool>> VerifyFaceAsync(string photoReference, string employeeId)
    {
        _logger.LogInformation("Verifying face for employee {EmployeeId} using photo {PhotoReference}", employeeId, photoReference);
        return _provider.VerifyFaceAsync(photoReference, employeeId);
    }
}
