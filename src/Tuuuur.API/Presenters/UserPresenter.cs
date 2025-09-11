using Tuuuur.Core.Responses;

namespace Tuuuur.API.Presenters
{
    /// <summary>
    ///
    /// </summary>
    public class UserPresenter : AResponseMessageJsonPresenter<UserResponse>
    {
        /// <inheritdoc />
        protected override object GetSuccessMember(UserResponse p_Response)
        {
            return p_Response.Value;
        }
    }
}