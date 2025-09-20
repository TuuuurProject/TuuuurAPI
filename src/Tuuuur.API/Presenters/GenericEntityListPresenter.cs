using Tuuuur.Core.Responses;
using Tuuuur.Domain.Bo;

namespace Tuuuur.API.Presenters;

/// <summary>
/// Generic entity list presenter
/// </summary>
/// <typeparam name="T"></typeparam>
public class GenericEntityListPresenter<T>:  AResponseMessageJsonPresenter<GenericEntityListResponse<T>> where T : class, IBOEntity
{
    /// <inheritdoc />
    protected override object GetSuccessMember(GenericEntityListResponse<T> p_Response)
    {
        return p_Response.Value;
    }
}