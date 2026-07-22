using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Infrastructure.Services.FaceRecognition.Providers;

public class NullFaceRecognitionProvider : IFaceRecognitionProvider
{
    public Task<Result<bool>> VerifyFaceAsync(string photoReference, string employeeId)
    {
        return Task.FromResult(Result<bool>.Failure(new[] { "Face recognition provider is not configured for production." }));
    }
}
