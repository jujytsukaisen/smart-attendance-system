using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Companies.Commands;

public class DeactivateCompanyCommand : IRequest<Result>
{
    public Guid CompanyId { get; set; }
}

public class DeactivateCompanyCommandValidator : AbstractValidator<DeactivateCompanyCommand>
{
    public DeactivateCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}

public class DeactivateCompanyCommandHandler : IRequestHandler<DeactivateCompanyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeactivateCompanyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeactivateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FindAsync(new object[] { request.CompanyId }, cancellationToken);

        if (company == null)
            return Result.Failure(new[] { "Company not found." });

        company.Deactivate();

        return Result.Success();
    }
}

