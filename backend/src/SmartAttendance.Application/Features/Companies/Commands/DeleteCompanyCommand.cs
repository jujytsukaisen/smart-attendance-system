using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Companies.Commands;

public class DeleteCompanyCommand : IRequest<Result>
{
    public Guid CompanyId { get; set; }
}

public class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
{
    public DeleteCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteCompanyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.FindAsync(new object[] { request.CompanyId }, cancellationToken);

        if (company == null)
            return Result.Failure(new[] { "Company not found." });

        company.Delete();

        return Result.Success();
    }
}



