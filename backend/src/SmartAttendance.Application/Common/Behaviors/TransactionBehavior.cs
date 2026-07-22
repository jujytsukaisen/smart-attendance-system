using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SmartAttendance.Application.Common.Interfaces;

namespace SmartAttendance.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IUnitOfWork unitOfWork,
        IApplicationDbContext dbContext,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Exclude Queries from Transactions, commonly identified by "Query" postfix in CQRS
        if (requestName.EndsWith("Query"))
        {
            return await next();
        }

        try
        {
            _logger.LogInformation("Beginning Transaction for {RequestName}", requestName);

            var response = await next();

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Committed Transaction for {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling transaction for {RequestName}", requestName);
            throw;
        }
    }
}
