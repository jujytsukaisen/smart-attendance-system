using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Infrastructure.Services.FaceRecognition.Providers;

public interface IFaceRecognitionProvider
{
    Task<Result<bool>> VerifyFaceAsync(string photoReference, string employeeId);
}
