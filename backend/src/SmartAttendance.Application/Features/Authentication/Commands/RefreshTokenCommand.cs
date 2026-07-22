using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Authentication.Commands;

public class RefreshTokenCommand : IRequest<Result<RefreshTokenResponse>>
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly SmartAttendance.Application.Common.Interfaces.IAuthenticationService _authService;

    public RefreshTokenCommandHandler(SmartAttendance.Application.Common.Interfaces.IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var authResponse = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse
        {
            Token = authResponse.AccessToken,
            RefreshToken = authResponse.RefreshToken
        });
    }
}

