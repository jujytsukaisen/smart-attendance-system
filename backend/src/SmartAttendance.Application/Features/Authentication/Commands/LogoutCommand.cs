using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Authentication.Commands;

public class LogoutCommand : IRequest<Result>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthenticationService _authService;

    public LogoutCommandHandler(ICurrentUserService currentUserService, IAuthenticationService authService)
    {
        _currentUserService = currentUserService;
        _authService = authService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(_currentUserService.UserId?.ToString() ?? string.Empty);
        if (!result) return Result.Failure(new[] { "Failed to logout." });
        return Result.Success();
    }
}

