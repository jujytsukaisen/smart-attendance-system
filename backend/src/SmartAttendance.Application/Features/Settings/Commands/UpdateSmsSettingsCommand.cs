using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Settings.Commands;

public class UpdateSmsSettingsCommand : IRequest<Result>
{
    public string Provider { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
}

public class UpdateSmsSettingsCommandValidator : AbstractValidator<UpdateSmsSettingsCommand>
{
    public UpdateSmsSettingsCommandValidator()
    {
        RuleFor(x => x.Provider).NotEmpty();
    }
}

public class UpdateSmsSettingsCommandHandler : IRequestHandler<UpdateSmsSettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateSmsSettingsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateSmsSettingsCommand request, CancellationToken cancellationToken)
    {
        var companyId = _context.CurrentTenantId;
        if (companyId == Guid.Empty) return Result.Failure(new[] { "Unable to determine company context." });

        await SettingsHelper.UpsertAsync(_context, companyId, "Sms", new Dictionary<string, string>
        {
            { "Provider", request.Provider },
            { "ApiKey", request.ApiKey },
            { "SenderName", request.SenderName },
        }, cancellationToken);

        return Result.Success();
    }
}
