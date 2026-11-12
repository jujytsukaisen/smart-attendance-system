using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Infrastructure.Options;

namespace SmartAttendance.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailOptions _defaultOptions;
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailService(
        ILogger<EmailService> logger, 
        IOptions<EmailOptions> options, 
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _defaultOptions = options.Value;
        _scopeFactory = scopeFactory;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);

        var host = _defaultOptions.Host;
        var port = _defaultOptions.Port;
        var username = _defaultOptions.Username;
        var password = _defaultOptions.Password;
        var fromEmail = _defaultOptions.FromEmail;
        var fromName = _defaultOptions.FromName;
        var enableSsl = true;

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<IApplicationDbContext>();

        if (dbContext != null && dbContext.CurrentTenantId != Guid.Empty)
        {
            var companyId = dbContext.CurrentTenantId;
            var settings = await dbContext.AppSettings
                .Where(s => s.CompanyId == companyId && s.Category == "Email")
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            if (settings.Any())
            {
                if (settings.TryGetValue("SmtpHost", out var h) && !string.IsNullOrWhiteSpace(h)) host = h;
                if (settings.TryGetValue("SmtpPort", out var pStr) && int.TryParse(pStr, out var p)) port = p;
                if (settings.TryGetValue("Username", out var u) && !string.IsNullOrWhiteSpace(u)) username = u;
                if (settings.TryGetValue("Password", out var pwd) && !string.IsNullOrWhiteSpace(pwd)) password = pwd;
                if (settings.TryGetValue("SenderEmail", out var se) && !string.IsNullOrWhiteSpace(se)) fromEmail = se;
                if (settings.TryGetValue("SenderName", out var sn) && !string.IsNullOrWhiteSpace(sn)) fromName = sn;
                if (settings.TryGetValue("EnableSsl", out var sslStr) && bool.TryParse(sslStr, out bool parsedSsl)) enableSsl = parsedSsl;
            }
        }

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = enableSsl
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
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
