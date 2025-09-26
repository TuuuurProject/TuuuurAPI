namespace Tuuuur.Domain.Notifications;

public interface INotificationsService
{
    Task PushMessageAsync<T>(ClientType p_ClientType, T p_Message, string p_User = null);
    Task PushMessageAsync(ClientType p_ClientType, string p_Message, string p_User = null);
}