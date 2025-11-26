namespace Tuuuur.Domain.Notifications;

public interface INotificationsService
{
    Task PushMessageAsync<T>(ClientType p_ClientType, T p_Message, IReadOnlyList<string> p_Users);
    Task PushMessageAsync(ClientType p_ClientType, string p_Message, IReadOnlyList<string> p_Users);
}