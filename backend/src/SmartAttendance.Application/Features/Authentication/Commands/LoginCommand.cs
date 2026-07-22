using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Application.Common.Models;

namespace SmartAttendance.Application.Features.Authentication.Commands;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool RequiresTwoFactor { get; set; }
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthenticationService _authService;

    public LoginCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var authResponse = await _authService.LoginAsync(request.Email, request.Password);
        
        return Result<LoginResponse>.Success(new LoginResponse
        {
            Token = authResponse.AccessToken,
            RefreshToken = authResponse.RefreshToken,
            RequiresTwoFactor = false
        });
    }
}
