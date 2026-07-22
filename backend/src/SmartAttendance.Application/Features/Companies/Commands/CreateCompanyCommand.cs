using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Companies.Commands;

public class CreateCompanyCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string AddressStreet { get; set; } = string.Empty;
    public string AddressCity { get; set; } = string.Empty;
    public string AddressState { get; set; } = string.Empty;
    public string AddressCountry { get; set; } = string.Empty;
    public string AddressZipCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FoundedDate { get; set; }
    public string? Website { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
}

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TaxNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressStreet).NotEmpty().MaximumLength(250);
        RuleFor(x => x.AddressCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressState).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressCountry).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressZipCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FoundedDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.Website).MaximumLength(200);
        RuleFor(x => x.TimeZone).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty();
    }
}

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateCompanyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var email = new SmartAttendance.Domain.ValueObjects.Email(request.Email);
            var phone = new SmartAttendance.Domain.ValueObjects.PhoneNumber(request.PhoneNumber);

            var address = new SmartAttendance.Domain.ValueObjects.Address(
                request.AddressStreet, 
                request.AddressCity, 
                request.AddressState, 
                request.AddressCountry, 
                request.AddressZipCode);

            var company = SmartAttendance.Domain.Entities.Company.Create(
                request.Name,
                request.LegalName,
                request.RegistrationNumber,
                request.TaxNumber,
                email,
                phone,
                address,
                request.FoundedDate,
                request.Website);

            company.UpdateSettings(request.TimeZone, request.Currency);

            await _context.Companies.AddAsync(company, cancellationToken);
            
            return Result<Guid>.Success(company.Id);
        }
        catch (System.Exception ex)
        {
            return Result<Guid>.Failure(new[] { ex.Message });
        }
    }
}

