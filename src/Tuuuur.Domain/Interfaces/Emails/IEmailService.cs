namespace Tuuuur.Domain.Interfaces.Emails;

public interface IEmailService
{
    Task SendAsync(string p_Subject, string p_Content, IEnumerable<string> p_To,
        IEnumerable<string> p_Cc = null, IEnumerable<string> p_Bcc = null, CancellationToken p_CancellationToken = default);
}
