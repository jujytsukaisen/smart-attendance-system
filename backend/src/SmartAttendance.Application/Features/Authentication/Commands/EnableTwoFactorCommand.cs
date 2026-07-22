using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Authentication.Commands;

public class EnableTwoFactorCommand : IRequest<Result>
{
    public string Code { get; set; } = string.Empty;
}

public class EnableTwoFactorCommandValidator : AbstractValidator<EnableTwoFactorCommand>
{
    public EnableTwoFactorCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
    }
}

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthenticationService _authService;

    public EnableTwoFactorCommandHandler(ICurrentUserService currentUserService, IAuthenticationService authService)
    {
        _currentUserService = currentUserService;
        _authService = authService;
    }

    public async Task<Result> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.EnableTwoFactorAsync(_currentUserService.UserId?.ToString() ?? string.Empty);
        if (!result) return Result.Failure(new[] { "Failed to enable two factor authentication." });
        return Result.Success();
    }
}

