using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartAttendance.Application.Common.Interfaces;

namespace SmartAttendance.Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public AuthorizationBehavior(
        ICurrentUserService currentUserService,
        IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Future robust authorization checks (e.g. reflection over [Authorize] attributes) will be executed here.
        // It relies on ICurrentUserService and IIdentityService for state validation.
        
        return await next();
    }
}
