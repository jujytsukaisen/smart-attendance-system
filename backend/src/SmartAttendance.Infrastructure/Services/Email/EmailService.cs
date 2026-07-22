using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Infrastructure.Options;

namespace SmartAttendance.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailOptions _options;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            Credentials = new NetworkCredential(_options.Username, _options.Password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        
        mailMessage.To.Add(to);

        // Async sending with basic retry simulation would go here
        try
        {
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw; 
        }
    }
}
