using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Infrastructure.Services.FaceRecognition.Providers;

public interface IFaceRecognitionProvider
{
    // يقارن بصمتين رياضيًا (Euclidean Distance) ويرجع تطابق/عدم تطابق + نسبة الثقة
    Task<Result<FaceMatchResult>> VerifyFaceAsync(float[] storedEncoding, float[] submittedEncoding, double threshold);
}