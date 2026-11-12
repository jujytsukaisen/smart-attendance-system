using System;
using System.Threading.Tasks;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Common.Interfaces;

public interface IFaceRecognitionService
{
    Task<Result<FaceMatchResult>> VerifyFaceAsync(Guid employeeId, float[] submittedEncoding);
}