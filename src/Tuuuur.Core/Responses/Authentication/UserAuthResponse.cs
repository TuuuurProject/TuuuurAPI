using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Responses.Authentication;

/// <summary>
/// Response for user auth
/// </summary>
public class UserAuthResponse: UseCaseResponseMessage<UserAuth>
{
    public UserAuthResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
    {
    }

    public UserAuthResponse(UserAuth p_Value, string p_Message = null) : base(p_Value, p_Message)
    {
    }
}