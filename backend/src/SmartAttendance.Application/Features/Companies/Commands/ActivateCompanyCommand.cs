using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Companies.Commands;

public class ActivateCompanyCommand : IRequest<Result>
{
    public Guid CompanyId { get; set; }
}

public class ActivateCompanyCommandValidator : AbstractValidator<ActivateCompanyCommand>
{
    public ActivateCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}

public class ActivateCompanyCommandHandler : IRequestHandler<ActivateCompanyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ActivateCompanyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ActivateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FindAsync(new object[] { request.CompanyId }, cancellationToken);

        if (company == null)
            return Result.Failure(new[] { "Company not found." });

        company.Activate();

        return Result.Success();
    }
}

