using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Responses;

public class UserResponse : UseCaseResponseMessage<User>
{
    public UserResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
    {
    }

    public UserResponse(User p_Value, string p_Message = null) : base(p_Value, p_Message)
    {
    }
}