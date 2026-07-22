using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class UpdateBranchCommand : IRequest<Result>
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string AddressStreet { get; set; } = string.Empty;
    public string AddressCity { get; set; } = string.Empty;
    public string AddressState { get; set; } = string.Empty;
    public string AddressCountry { get; set; } = string.Empty;
    public string AddressZipCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TimeZone { get; set; } = "UTC";
}

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.AddressStreet).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AddressCity).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressState).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressCountry).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AddressZipCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.TimeZone).NotEmpty();
    }
}

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateBranchCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);

        if (branch == null)
            return Result.Failure(new[] { "Branch not found." });

        branch.UpdateInformation(request.Name, request.Code, request.TimeZone);

        var address = new SmartAttendance.Domain.ValueObjects.Address(
            request.AddressStreet, request.AddressCity, request.AddressState, request.AddressCountry, request.AddressZipCode);

        branch.ChangeLocation(address, request.Latitude, request.Longitude);

        var email = new SmartAttendance.Domain.ValueObjects.Email(request.Email);
        var phone = new SmartAttendance.Domain.ValueObjects.PhoneNumber(request.PhoneNumber);

        branch.UpdateContactInformation(phone, email);

        return Result.Success();
    }
}



