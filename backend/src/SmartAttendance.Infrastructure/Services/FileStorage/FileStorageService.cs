using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;
using SmartAttendance.Infrastructure.Options;

namespace SmartAttendance.Infrastructure.Services.FileStorage;

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly StorageOptions _options;

    public FileStorageService(ILogger<FileStorageService> logger, IOptions<StorageOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string> UploadFileAsync(byte[] fileData, string fileName)
    {
        try
        {
            // 1. Sanitize file name to prevent path traversal
            var safeFileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                throw new ArgumentException("Invalid file name.");
            }

            // 2. Validate extension
            var ext = Path.GetExtension(safeFileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".pdf" };
            if (!System.Linq.Enumerable.Contains(allowedExtensions, ext))
            {
                throw new ArgumentException($"Extension {ext} is not allowed.");
            }

            // 3. Validate magic bytes (simplified)
            if (fileData == null || fileData.Length < 4)
            {
                throw new ArgumentException("File is empty or invalid.");
            }
            
            bool isValidMagicBytes = false;
            if (ext == ".png" && fileData[0] == 0x89 && fileData[1] == 0x50 && fileData[2] == 0x4E && fileData[3] == 0x47) isValidMagicBytes = true;
            else if ((ext == ".jpg" || ext == ".jpeg") && fileData[0] == 0xFF && fileData[1] == 0xD8 && fileData[2] == 0xFF) isValidMagicBytes = true;
            else if (ext == ".pdf" && fileData[0] == 0x25 && fileData[1] == 0x50 && fileData[2] == 0x44 && fileData[3] == 0x46) isValidMagicBytes = true;

            if (!isValidMagicBytes)
            {
                throw new ArgumentException("File content does not match extension.");
            }

            var directory = _options.LocalStoragePath;
            if (string.IsNullOrWhiteSpace(directory)) directory = Path.Combine(Path.GetTempPath(), "SmartAttendance");
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
            var filePath = Path.Combine(directory, uniqueFileName);

            await File.WriteAllBytesAsync(filePath, fileData);

            _logger.LogInformation("File uploaded successfully to {FilePath}", filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName}", fileName);
            throw;
        }
    }
}
