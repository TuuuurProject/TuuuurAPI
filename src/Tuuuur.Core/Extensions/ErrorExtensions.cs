using Tuuuur.Core.Responses;
using Tuuuur.Domain.Errors;

namespace Tuuuur.Core.Extensions;

public static class ErrorExtensions
{
    public static ErrorDto ToError(this Exception p_Ex)
    {
        return new ErrorDto(DomainErrors.UnknowError, p_Ex.Message, p_Ex);
    }

    public static ErrorDto ToError(this Exception p_Ex, string p_ErrorCode)
    {
        return new ErrorDto(p_ErrorCode, p_Ex.Message, p_Ex);
    }
}