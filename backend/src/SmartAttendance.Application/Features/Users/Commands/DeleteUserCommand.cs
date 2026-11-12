using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Users.Commands;

public class DeleteUserCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var domainUser = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (domainUser != null)
        {
            _context.Users.Remove(domainUser);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var result = await _identityService.DeleteUserByDomainIdAsync(request.UserId);
        return result;
    }
}
