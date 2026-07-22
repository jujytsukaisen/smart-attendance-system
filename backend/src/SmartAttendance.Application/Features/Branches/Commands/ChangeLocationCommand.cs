using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Branches.Commands;

public class ChangeLocationCommand : IRequest<Result>
{
    public Guid BranchId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
}

public class ChangeLocationCommandValidator : AbstractValidator<ChangeLocationCommand>
{
    public ChangeLocationCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(250);
    }
}

public class ChangeLocationCommandHandler : IRequestHandler<ChangeLocationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeLocationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeLocationCommand request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FindAsync(new object[] { request.BranchId }, cancellationToken);

        if (branch == null)
            return Result.Failure(new[] { "Branch not found." });

        var address = new SmartAttendance.Domain.ValueObjects.Address(
            request.Address, "City", "State", "Country", "00000");

        branch.ChangeLocation(address, request.Latitude, request.Longitude);

        return Result.Success();
    }
}

