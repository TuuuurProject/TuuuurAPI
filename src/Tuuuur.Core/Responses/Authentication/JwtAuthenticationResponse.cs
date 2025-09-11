using Tuuuur.Domain.Token;

namespace Tuuuur.Core.Responses.Authentication;

public class JwtAuthenticationResponse : UseCaseResponseMessage<UserToken>
{
    public JwtAuthenticationResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
    {
    }

    public JwtAuthenticationResponse(UserToken p_Value, string p_Message = null) : base(p_Value, p_Message)
    {
    }
}