using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Common.Interfaces;

public interface IFaceRecognitionService
{
    Task<Result<bool>> VerifyFaceAsync(string photoReference, string employeeId);
}
