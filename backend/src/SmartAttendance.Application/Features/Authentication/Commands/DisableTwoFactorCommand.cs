using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Authentication.Commands;

public class DisableTwoFactorCommand : IRequest<Result>
{
    public string Code { get; set; } = string.Empty;
}

public class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
    }
}

public class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthenticationService _authService;

    public DisableTwoFactorCommandHandler(ICurrentUserService currentUserService, IAuthenticationService authService)
    {
        _currentUserService = currentUserService;
        _authService = authService;
    }

    public async Task<Result> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.DisableTwoFactorAsync(_currentUserService.UserId?.ToString() ?? string.Empty);
        if (!result) return Result.Failure(new[] { "Failed to disable two factor authentication." });
        return Result.Success();
    }
}

