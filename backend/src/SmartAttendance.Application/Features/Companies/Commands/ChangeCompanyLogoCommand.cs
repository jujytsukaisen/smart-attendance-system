using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Companies.Commands;

public class ChangeCompanyLogoCommand : IRequest<Result>
{
    public Guid CompanyId { get; set; }
    public string LogoUrl { get; set; } = string.Empty;
}

public class ChangeCompanyLogoCommandValidator : AbstractValidator<ChangeCompanyLogoCommand>
{
    public ChangeCompanyLogoCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.LogoUrl).NotEmpty();
    }
}

public class ChangeCompanyLogoCommandHandler : IRequestHandler<ChangeCompanyLogoCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeCompanyLogoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeCompanyLogoCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FindAsync(new object[] { request.CompanyId }, cancellationToken);

        if (company == null)
            return Result.Failure(new[] { "Company not found." });

        company.ChangeLogo(request.LogoUrl);

        return Result.Success();
    }
}

