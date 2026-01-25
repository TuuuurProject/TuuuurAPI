using Tuuuur.Core.Responses;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tuuuur.Domain.Errors;

namespace Tuuuur.API.Presenters
{
    /// <summary>
    /// Presenter meant to be used to return content as JSON
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AResponseMessageJsonPresenter<T> : AJsonPresenter<T>
        where T : UseCaseResponseMessage
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected AResponseMessageJsonPresenter() : base()
        {
        }

        /// <summary>
        /// Handle the response convertion to JSON
        /// </summary>
        /// <param name="p_Response"></param>
        public override void Handle(T p_Response)
        {
            ContentResult.StatusCode = (int)GetStatusCode(p_Response);

            JsonSerializerOptions v_JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
            };

            ContentResult.Content = p_Response.Success ? JsonSerializer.Serialize(GetSuccessMember(p_Response), v_JsonSerializerOptions) : JsonSerializer.Serialize(p_Response.Errors, v_JsonSerializerOptions);
        }

        /// <summary>
        /// Logic to determine status code for the response
        /// </summary>
        /// <param name="p_Response"></param>
        /// <returns></returns>
        protected override HttpStatusCode GetStatusCode(T p_Response)
        {
            // Customize here status code -based on errors

            //Case where the data cannot be found
            if (p_Response.Errors?.Any() ?? false)
            {
                return p_Response.Errors.FirstOrDefault()?.Code switch
                {
                    DomainErrors.Data.NotFound => HttpStatusCode.NotFound,
                    DomainErrors.Data.AlreadyExist => HttpStatusCode.Conflict,
                    DomainErrors.Authentication.Invalid => HttpStatusCode.Unauthorized,
                    DomainErrors.Authentication.RefreshToken.Invalid => HttpStatusCode.Unauthorized,
                    _ => HttpStatusCode.InternalServerError,
                };
            }

            return HttpStatusCode.OK;
        }
    }
}