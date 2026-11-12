using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    private readonly IApplicationDbContext _context;

    public FaceRecognitionService(
        ILogger<FaceRecognitionService> logger,
        IOptions<FaceRecognitionOptions> options,
        IFaceRecognitionProvider provider,
        IApplicationDbContext context)
    {
        _logger = logger;
        _options = options.Value;
        _provider = provider;
        _context = context;
    }

    public async Task<Result<FaceMatchResult>> VerifyFaceAsync(Guid employeeId, float[] submittedEncoding)
    {
        _logger.LogInformation("Verifying face for employee {EmployeeId}", employeeId);

        var profile = await _context.FaceProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.IsActive);

        if (profile is null || string.IsNullOrWhiteSpace(profile.FaceEncoding))
        {
            return Result<FaceMatchResult>.Failure(new[] { "No active face profile registered for this employee." });
        }

        var storedEncoding = JsonSerializer.Deserialize<float[]>(profile.FaceEncoding);
        if (storedEncoding is null)
        {
            return Result<FaceMatchResult>.Failure(new[] { "Stored face encoding is corrupted." });
        }

        // ✅ قراءة عتبة التعرف من إعدادات قاعدة البيانات (AppSettings) بدلاً من appsettings.json الثابت
        // هذا يجعل إعداد "Recognition Threshold" في صفحة الإعدادات يعمل فعلاً
        var threshold = await GetThresholdFromDbAsync();
        _logger.LogInformation("Using recognition threshold: {Threshold} for employee {EmployeeId}", threshold, employeeId);

        return await _provider.VerifyFaceAsync(storedEncoding, submittedEncoding, threshold);
    }

    /// <summary>
    /// يقرأ عتبة التعرف من جدول AppSettings.
    /// الإعداد يُخزَّن كنسبة مئوية (مثلاً 80 = 80%) ويُحوَّل إلى مسافة إقليدية (0.0-1.0).
    /// إذا لم يُوجد في DB يستخدم القيمة الافتراضية من appsettings.json.
    /// </summary>
    private async Task<double> GetThresholdFromDbAsync()
    {
        try
        {
            var companyId = _context.CurrentTenantId;
            if (companyId == Guid.Empty) return _options.RecognitionThreshold;

            var setting = await _context.AppSettings
                .AsNoTracking()
                .Where(s => s.CompanyId == companyId
                         && s.Category == "FaceRecognition"
                         && s.Key == "RecognitionThreshold")
                .Select(s => s.Value)
                .FirstOrDefaultAsync();

            if (setting is null) return _options.RecognitionThreshold;

            if (!double.TryParse(setting, out var raw)) return _options.RecognitionThreshold;

            // الإعداد مخزون كنسبة مئوية (0-100) — نحوله لمسافة إقليدية (0.0-1.0)
            // مثال: 80% → threshold = 1 - (80/100) = 0.20 (مسافة أقصاها 0.20)
            if (raw > 1.0) raw = raw / 100.0;

            // عتبة المسافة الإقليدية = (1 - الثقة المطلوبة)
            // كلما زادت الثقة المطلوبة → قلّت المسافة المسموحة → تشدّدت الدقة
            return 1.0 - raw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read RecognitionThreshold from DB, using default: {Default}", _options.RecognitionThreshold);
            return _options.RecognitionThreshold;
        }
    }
}