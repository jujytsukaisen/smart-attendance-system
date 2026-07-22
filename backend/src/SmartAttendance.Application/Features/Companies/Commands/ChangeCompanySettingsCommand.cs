using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Companies.Commands;

public class ChangeCompanySettingsCommand : IRequest<Result>
{
    public Guid CompanyId { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}

public class ChangeCompanySettingsCommandValidator : AbstractValidator<ChangeCompanySettingsCommand>
{
    public ChangeCompanySettingsCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.TimeZone).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty();
    }
}

public class ChangeCompanySettingsCommandHandler : IRequestHandler<ChangeCompanySettingsCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeCompanySettingsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeCompanySettingsCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FindAsync(new object[] { request.CompanyId }, cancellationToken);

        if (company == null)
            return Result.Failure(new[] { "Company not found." });

        company.UpdateSettings(request.TimeZone, request.Currency);

        return Result.Success();
    }
}

