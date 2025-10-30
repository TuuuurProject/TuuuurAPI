using Ardalis.GuardClauses;
using MediatR;
using Tuuuur.Domain.Interfaces.Data;
using Microsoft.Extensions.Logging;
using Tuuuur.Core.Responses;
using Tuuuur.Domain.Errors;

namespace Tuuuur.Core.UseCases;

internal abstract class ADbUseCase<TRequest, TResponse>(ILogger p_Logger, IUnitOfWork p_UnitOfWork)
    : AUseCase<TRequest, TResponse>(p_Logger)
    where TRequest : IRequest<TResponse>
{
    protected readonly IUnitOfWork m_UnitOfWork = p_UnitOfWork;
}

internal abstract class AUseCase<TRequest, TResponse>(ILogger p_Logger) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly ILogger m_Logger = p_Logger;

    [SuppressMessage("Style", "IDE1006:Styles d'affectation de noms", Justification = "Inherited named")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await HandleLogic(request, cancellationToken);
        }
        catch (Exception v_Exception)
        {
            return await HandleException(v_Exception, request, cancellationToken);
        }
    }

    protected abstract Task<TResponse> HandleLogic(TRequest p_Request, CancellationToken p_CancellationToken);
    
    protected virtual Task<TResponse> HandleException(Exception p_Exception, TRequest p_Request,
        CancellationToken p_CancellationToken)
    {
        const string v_ErrorDescription = "An unknown error occurred";

        m_Logger.LogError(p_Exception, v_ErrorDescription);

        return Task.FromResult((TResponse)Activator.CreateInstance(
            typeof(TResponse),
            new List<ErrorDto> { new(DomainErrors.UnknowError, v_ErrorDescription, p_Exception) },
            v_ErrorDescription
        ));
    }
}