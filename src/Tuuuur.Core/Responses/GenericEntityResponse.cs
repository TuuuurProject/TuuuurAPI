using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Responses;

public class GenericEntityResponse<T> : UseCaseResponseMessage<T> where T : class, IBOEntity
{
    public GenericEntityResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
    {
    }

    public GenericEntityResponse(T p_Value, string p_Message = null) : base(p_Value, p_Message)
    {
    }
}