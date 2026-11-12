using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Users.Commands;

public class CreateUserCommand : IRequest<Result<Guid>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateUserCommandHandler(
        IIdentityService identityService, 
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.TenantId, out var companyId))
        {
            return Result<Guid>.Failure(new[] { "Invalid Tenant ID." });
        }

        var email = new SmartAttendance.Domain.ValueObjects.Email(request.Email);
        var domainUser = SmartAttendance.Domain.Entities.User.Create(companyId, request.Username, email);

        _context.Users.Add(domainUser);
        await _context.SaveChangesAsync(cancellationToken);

        var (result, userId) = await _identityService.CreateUserAsync(domainUser.Id, request.Username, request.Password);
        
        if (!result.Succeeded)
        {
            _context.Users.Remove(domainUser);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Failure(result.Errors);
        }

        return Result<Guid>.Success(domainUser.Id);
    }
}
