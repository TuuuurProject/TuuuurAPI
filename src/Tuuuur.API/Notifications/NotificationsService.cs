using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Tuuuur.Domain.Notifications;

namespace Tuuuur.API.Notifications;

internal class NotificationsService(
    IHubContext<NotificationsHub, INotificationClient> p_HubContext)
    : INotificationsService
{
    private readonly JsonSerializerOptions m_JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public Task PushMessageAsync<T>(ClientType p_ClientType, T p_Message, string p_User = null)
        {
            string v_Message = JsonSerializer.Serialize(p_Message, m_JsonSerializerOptions);

            return PushMessageAsync(p_ClientType, v_Message, p_User);
        }
        public Task PushMessageAsync(ClientType p_ClientType, string p_Message, string p_User = null)
        {
            if (p_ClientType is ClientType.AllExceptUser or ClientType.User &&
                string.IsNullOrWhiteSpace(p_User))
                throw new ArgumentNullException(p_User);

            return p_ClientType switch
            {
                ClientType.All => PushMessageToAllAsync(p_Message),
                ClientType.AllExceptUser => PushMessageToAllExceptUserAsync(p_User, p_Message),
                ClientType.User => PushMessageToUserAsync(p_User, p_Message),
                _ => throw new InvalidOperationException($"Unhandled ClientType: {p_ClientType}"),
            };
        }

        /// <summary>
        /// Push a message to a specific user
        /// </summary>
        /// <param name="p_User"></param>
        /// <param name="p_Message"></param>
        /// <returns></returns>
        private async Task PushMessageToUserAsync(string p_User, string p_Message)
        {
            await p_HubContext.Clients.User(p_User).Notify(p_Message);
        }

        /// <summary>
        /// Push a message to all connected users except the one provided
        /// </summary>
        /// <param name="p_User"></param>
        /// <param name="p_Message"></param>
        /// <returns></returns>
        private async Task PushMessageToAllExceptUserAsync(string p_User, string p_Message)
        {
            await p_HubContext.Clients.AllExcept(p_User).Notify(p_Message);
        }

        /// <summary>
        /// Push a message to all connected users
        /// </summary>
        /// <param name="p_Message"></param>
        /// <returns></returns>
        private async Task PushMessageToAllAsync(string p_Message)
        {
            await p_HubContext.Clients.All.Notify(p_Message);
        }
    }