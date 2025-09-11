using Tuuuur.Domain.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace Tuuuur.Core.UseCases;

internal abstract class AUseCase
{
    protected readonly IUnitOfWork m_UnitOfWork;
    protected readonly ILogger m_Logger;

    protected AUseCase(IUnitOfWork p_UnitOfWork, ILogger p_Logger)
    {
        m_UnitOfWork = p_UnitOfWork;
        m_Logger = p_Logger;
    }
}