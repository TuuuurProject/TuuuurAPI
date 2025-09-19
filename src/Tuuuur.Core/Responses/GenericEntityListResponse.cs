using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Responses;

public class GenericEntityListResponse<T> : UseCaseResponseMessage<IEnumerable<IBOEntity>> where T : IBOEntity
{
    public GenericEntityListResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
    {
    }

    public GenericEntityListResponse(IEnumerable<IBOEntity> p_Value, string p_Message = null) : base(p_Value, p_Message)
    {
    }
}