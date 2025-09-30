using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Responses;

public class GuidResponse : UseCaseResponseMessage<Guid>
{
    public GuidResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
    {
    }

    public GuidResponse(Guid p_Value, string p_Message = null) : base(p_Value, p_Message)
    {
    }
}