namespace Tuuuur.Core.Responses;

/// <summary>
/// Generic response for non-entity operations
/// </summary>
/// <typeparam name="T">The response data type</typeparam>
public class GenericResponse<T> : UseCaseResponseMessage<T> where T : class
{
    public GenericResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
    {
    }

    public GenericResponse(T p_Value, string p_Message = null) : base(p_Value, p_Message)
    {
    }
}
