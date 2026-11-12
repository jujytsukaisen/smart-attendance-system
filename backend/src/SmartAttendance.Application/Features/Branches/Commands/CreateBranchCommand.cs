using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class CreateBranchCommand : IRequest<Result<Guid>>
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string AddressStreet { get; set; } = string.Empty;
    public string AddressCity { get; set; } = string.Empty;
    public string AddressState { get; set; } = string.Empty;
    public string AddressCountry { get; set; } = string.Empty;
    public string AddressZipCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? AllowedRadiusMeters { get; set; }
    public string? TimeZone { get; set; }
    public bool IsHeadOffice { get; set; }
}

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.AddressStreet).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressCity).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressState).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressCountry).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressZipCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.AllowedRadiusMeters).GreaterThan(0).When(x => x.AllowedRadiusMeters.HasValue);
    }
}

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateBranchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var companyExists = await _context.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
            return Result<Guid>.Failure(new[] { "Company not found." });

        var address = new SmartAttendance.Domain.ValueObjects.Address(
            request.AddressStreet, request.AddressCity, request.AddressState, request.AddressCountry, request.AddressZipCode);

        var email = new SmartAttendance.Domain.ValueObjects.Email(request.Email);
        var phone = new SmartAttendance.Domain.ValueObjects.PhoneNumber(request.PhoneNumber);

        var branch = SmartAttendance.Domain.Entities.Branch.Create(
            request.CompanyId,
            request.Name,
            request.Code,
            address,
            phone,
            email,
            request.Latitude,
            request.Longitude,
            request.AllowedRadiusMeters,
            request.TimeZone,
            request.IsHeadOffice);

        _context.Branches.Add(branch);

        return Result<Guid>.Success(branch.Id);
    }
}

