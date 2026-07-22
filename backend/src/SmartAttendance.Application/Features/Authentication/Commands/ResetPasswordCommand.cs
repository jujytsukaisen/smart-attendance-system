using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Authentication.Commands;

public class ResetPasswordCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>    
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly SmartAttendance.Application.Common.Interfaces.IAuthenticationService _authService;

    public ResetPasswordCommandHandler(SmartAttendance.Application.Common.Interfaces.IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        if (!result) return Result.Failure(new[] { "Failed to reset password." });
        return Result.Success();
    }
}
