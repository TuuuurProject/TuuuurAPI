using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Tuuuur.Domain.Interfaces.Emails;

namespace Tuuuur.Infrastructure.Emails;

internal class EmailService(SmtpEmailConfiguration p_SmtpEmailConfiguration, ILogger<EmailService> p_Logger)
    : IEmailService
{
    public async Task SendAsync(
        string p_Subject, 
        string p_Content, 
        IEnumerable<string> p_To, 
        IEnumerable<string> p_Cc = null, 
        IEnumerable<string> p_Bcc = null, 
        IDictionary<string, string> p_InlineImages = null, 
        CancellationToken p_CancellationToken = default)
    {
        MimeMessage v_Email = new();

        v_Email.From.Add(new MailboxAddress(p_SmtpEmailConfiguration.FromName, p_SmtpEmailConfiguration.FromAddress));
        v_Email.To.AddRange(p_To.Select(MailboxAddress.Parse));

        if (p_Cc?.Any() == true)
            v_Email.Cc.AddRange(p_Cc.Select(MailboxAddress.Parse));

        if (p_Bcc?.Any() == true)
            v_Email.Bcc.AddRange(p_Bcc.Select(MailboxAddress.Parse));

        v_Email.Subject = p_Subject;
        
        BodyBuilder v_BodyBuilder = new BodyBuilder
        {
            HtmlBody = p_Content
        };

        if (p_InlineImages != null)
        {
            foreach (KeyValuePair<string, string> v_Kvp in p_InlineImages)
            {
                string v_ContentId = v_Kvp.Key;
                string v_FilePath = v_Kvp.Value;

                if (File.Exists(v_FilePath))
                {
                    MimeEntity v_Resource = await v_BodyBuilder.LinkedResources.AddAsync(v_FilePath, p_CancellationToken);
                    v_Resource.ContentId = v_ContentId;
                }
            }
        }

        v_Email.Body = v_BodyBuilder.ToMessageBody();

        using SmtpClient v_Smtp = new();
        await v_Smtp.ConnectAsync(p_SmtpEmailConfiguration.SmtpAddress, p_SmtpEmailConfiguration.SmtpPort, p_SmtpEmailConfiguration.EnableEncryption ? SecureSocketOptions.Auto : SecureSocketOptions.None, p_CancellationToken);

        if (!string.IsNullOrWhiteSpace(p_SmtpEmailConfiguration.SmtpLogin) && !string.IsNullOrWhiteSpace(p_SmtpEmailConfiguration.SmtpPassword))
            await v_Smtp.AuthenticateAsync(p_SmtpEmailConfiguration.SmtpLogin, p_SmtpEmailConfiguration.SmtpPassword, p_CancellationToken);

        string v_SmtpResponse = await v_Smtp.SendAsync(v_Email, p_CancellationToken);

        p_Logger.LogInformation("Sending email, Smtp response : {SmtpResponse}", v_SmtpResponse);

        await v_Smtp.DisconnectAsync(true, p_CancellationToken);
    }
}
