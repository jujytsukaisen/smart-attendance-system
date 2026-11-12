using System;
using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Infrastructure.Services.FaceRecognition.Providers;

// ✅ التطبيق الحقيقي: يستبدل NullFaceRecognitionProvider
// يقارن بصمتي الوجه (128-d vectors من face-api.js) عبر Euclidean Distance
// كل الحساب محلي بدون أي خدمة خارجية أو تكلفة
public class LocalFaceRecognitionProvider : IFaceRecognitionProvider
{
    public Task<Result<FaceMatchResult>> VerifyFaceAsync(float[] storedEncoding, float[] submittedEncoding, double threshold)
    {
        if (storedEncoding is null || storedEncoding.Length == 0)
            return Task.FromResult(Result<FaceMatchResult>.Failure(new[] { "No stored face encoding found for this employee." }));

        if (submittedEncoding is null || submittedEncoding.Length == 0)
            return Task.FromResult(Result<FaceMatchResult>.Failure(new[] { "Submitted face encoding is empty." }));

        if (storedEncoding.Length != submittedEncoding.Length)
            return Task.FromResult(Result<FaceMatchResult>.Failure(new[] { "Face encoding dimension mismatch." }));

        var distance = EuclideanDistance(storedEncoding, submittedEncoding);

        // face-api.js: مسافة أقل من ~0.6 تعتبر تطابق عادةً (threshold قابل للتعديل من الإعدادات)
        var isMatch = distance <= threshold;

        // تحويل تقريبي للمسافة إلى نسبة ثقة (0 = بعيد جدًا، threshold = حد التطابق)
        var confidence = Math.Max(0, Math.Min(100, (1 - (distance / (threshold * 2))) * 100));

        var result = new FaceMatchResult
        {
            IsMatch = isMatch,
            Distance = Math.Round(distance, 4),
            Confidence = Math.Round(confidence, 1)
        };

        return Task.FromResult(Result<FaceMatchResult>.Success(result));
    }

    private static double EuclideanDistance(float[] a, float[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
        {
            var diff = a[i] - b[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }
}