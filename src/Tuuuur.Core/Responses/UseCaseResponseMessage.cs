using Tuuuur.Domain.Bo;

namespace Tuuuur.Core.Responses
{
    public abstract class UseCaseResponseMessage
    {
        public bool Success { get; }
        public string Message { get; }
        public IEnumerable<ErrorDto> Errors { get; }

        protected UseCaseResponseMessage(IEnumerable<ErrorDto> p_Errors, string p_Message = null)
        {
            Errors = p_Errors;
            Success = false;
            Message = p_Message;
        }

        protected UseCaseResponseMessage(string p_Message = null)
        {
            Success = true;
            Message = p_Message;
        }
    }

    public abstract class UseCaseResponseMessage<T> : UseCaseResponseMessage
        where T : class
    {
        public T Value { get; set; }

        protected UseCaseResponseMessage(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        protected UseCaseResponseMessage(T p_Value, string p_Message = null) : base(p_Message)
        {
            Value = p_Value;
        }
    }
}