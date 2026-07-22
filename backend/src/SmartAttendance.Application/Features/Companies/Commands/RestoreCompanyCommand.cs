using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Companies.Commands;

public class RestoreCompanyCommand : IRequest<Result>
{
    public Guid CompanyId { get; set; }
}

public class RestoreCompanyCommandValidator : AbstractValidator<RestoreCompanyCommand>
{
    public RestoreCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}

public class RestoreCompanyCommandHandler : IRequestHandler<RestoreCompanyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RestoreCompanyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RestoreCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

        if (company == null)
            return Result.Failure(new[] { "Company not found." });

        if (!company.IsDeleted)
            return Result.Failure(new[] { "Company is not deleted." });

        company.Restore();

        return Result.Success();
    }
}
