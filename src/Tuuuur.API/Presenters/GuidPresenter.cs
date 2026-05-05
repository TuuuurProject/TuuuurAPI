using Tuuuur.Core.Responses;

namespace Tuuuur.API.Presenters
{
    /// <summary>
    ///
    /// </summary>
    public class GuidPresenter : AResponseMessageJsonPresenter<GuidResponse>
    {
        /// <inheritdoc />
        protected override object GetSuccessMember(GuidResponse p_Response)
        {
            return p_Response.Value;
        }
    }
}