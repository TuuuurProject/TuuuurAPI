using Tuuuur.Core.Responses;
using System.Text.Json.Nodes;

namespace Tuuuur.Core.Requests
{
    public class JsonResponse : UseCaseResponseMessage<JsonNode>
    {
        public JsonResponse(IEnumerable<ErrorDto> p_Errors, string p_Message = null) : base(p_Errors, p_Message)
        {
        }

        public JsonResponse(JsonNode p_Value, string p_Message = null) : base(p_Value, p_Message)
        {
        }
    }
}