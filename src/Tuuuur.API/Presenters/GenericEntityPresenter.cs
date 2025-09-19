using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Presenters;

/// <summary>
/// Generic presenter for entity
/// </summary>
/// <typeparam name="T"></typeparam>
public class GenericEntityPresenter<T>:  AResponseMessageJsonPresenter<GenericEntityResponse<T>> where T : class, IBOEntity
{
    /// <inheritdoc />
    protected override object GetSuccessMember(GenericEntityResponse<T> p_Response)
    {
        return p_Response.Value;
    }
}