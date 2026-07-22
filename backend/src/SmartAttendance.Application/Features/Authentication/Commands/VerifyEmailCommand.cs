using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Authentication.Commands;

public class VerifyEmailCommand : IRequest<Result>
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly SmartAttendance.Application.Common.Interfaces.IAuthenticationService _authService;

    public VerifyEmailCommandHandler(SmartAttendance.Application.Common.Interfaces.IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.ConfirmEmailAsync(request.UserId, request.Token);
        if (!result) return Result.Failure(new[] { "Failed to verify email." });
        return Result.Success();
    }
}
