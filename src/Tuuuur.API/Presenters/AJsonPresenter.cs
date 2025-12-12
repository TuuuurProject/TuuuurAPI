using Tuuuur.Domain.Configuration;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tuuuur.API.Presenters
{
    /// <summary>
    /// Singleton for Json Serializer Options
    /// </summary>
    public class SingletonJsonSerializerOptions
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected SingletonJsonSerializerOptions()
        {
        }

        /// <summary>
        /// Json Serializer Options
        /// </summary>
        protected static readonly JsonSerializerOptions m_JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }

    /// <summary>
    /// Base class for JSON data presentation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AJsonPresenter<T> : SingletonJsonSerializerOptions, IOutputPort<T>
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected AJsonPresenter()
        {
            ContentResult = new JsonContentResult();
        }

        /// <summary>
        /// Returned content
        /// </summary>
        public JsonContentResult ContentResult { get; }

        /// <summary>
        /// Handle the transformation from the response into JSON presentable data
        /// </summary>
        /// <param name="p_Response"></param>
        public virtual void Handle(T p_Response)
        {
            ContentResult.StatusCode = (int)GetStatusCode(p_Response);
            ContentResult.Content = JsonSerializer.Serialize(GetSuccessMember(p_Response), m_JsonSerializerOptions);
        }

        /// <summary>
        /// Get the member to use in case of success to serialize it
        /// </summary>
        /// <param name="p_Response">Use case reponse</param>
        /// <returns>Value to serialize as object</returns>
        protected abstract object GetSuccessMember(T p_Response);

        /// <summary>
        /// Abstract method used by derived class to get status code based on their own logic
        /// </summary>
        /// <param name="p_Response"></param>
        /// <returns></returns>
        protected abstract HttpStatusCode GetStatusCode(T p_Response);
    }
}