using System.Threading.Tasks;

namespace SmartAttendance.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(byte[] fileData, string fileName);
}
