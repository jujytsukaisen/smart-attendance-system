using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Authentication.Commands;

public class ChangePasswordCommand : IRequest<Result>
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthenticationService _authService;

    public ChangePasswordCommandHandler(ICurrentUserService currentUserService, IAuthenticationService authService)
    {
        _currentUserService = currentUserService;
        _authService = authService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.ChangePasswordAsync(_currentUserService.UserId?.ToString() ?? string.Empty, request.CurrentPassword, request.NewPassword);
        if (!result) return Result.Failure(new[] { "Failed to change password." });
        return Result.Success();
    }
}

