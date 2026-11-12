using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Settings.Commands;

public class UpdateEmailSettingsCommand : IRequest<Result>
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateEmailSettingsCommandValidator : AbstractValidator<UpdateEmailSettingsCommand>
{
    public UpdateEmailSettingsCommandValidator()
    {
        RuleFor(x => x.SmtpHost).NotEmpty();
        RuleFor(x => x.SmtpPort).InclusiveBetween(1, 65535);
        RuleFor(x => x.SenderEmail).NotEmpty().EmailAddress();
    }
}

public class UpdateEmailSettingsCommandHandler : IRequestHandler<UpdateEmailSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmailSettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateEmailSettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty) return Result.Failure(new[] { "Unable to determine company context." });

        await SettingsHelper.UpsertAsync(_context, companyId, "Email", new Dictionary<string, string>
        {
            { "SmtpHost", request.SmtpHost },
            { "SmtpPort", request.SmtpPort.ToString() },
            { "EnableSsl", request.EnableSsl.ToString() },
            { "Username", request.Username },
            { "SenderName", request.SenderName },
            { "SenderEmail", request.SenderEmail },
            { "Password", request.Password },
        }, cancellationToken);

        return Result.Success();
    }
}
