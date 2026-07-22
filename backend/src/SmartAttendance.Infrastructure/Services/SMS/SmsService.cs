using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAttendance.Infrastructure.Options;

namespace SmartAttendance.Infrastructure.Services.SMS;

public interface ISmsService
{
    Task SendSmsAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly SmsOptions _options;

    public SmsService(ILogger<SmsService> logger, IOptions<SmsOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendSmsAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending SMS to {ToPhoneNumber} via Twilio abstraction", toPhoneNumber);
        
        // Prepare Twilio Implementation
        // ...

        return Task.CompletedTask;
    }
}
