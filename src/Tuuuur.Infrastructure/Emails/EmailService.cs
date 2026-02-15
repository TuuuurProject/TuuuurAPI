using System.Text.RegularExpressions;
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
                string v_Value = v_Kvp.Value;

                // Check if it's a base64 encoded image (data:image/png;base64,...)
                if (v_Value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Extract format and base64 content
                        string[] v_Parts = v_Value.Split(",");
                        if (v_Parts.Length == 2)
                        {
                            string v_Header = v_Parts[0];
                            string v_Base64 = v_Parts[1];

                            Match v_TypeMatch = Regex.Match(v_Header, @"image/(\w+)", RegexOptions.None, TimeSpan.FromMilliseconds(100));
                            string v_ImageType = v_TypeMatch.Success ? v_TypeMatch.Groups[1].Value : "png";

                            byte[] v_ImageBytes = Convert.FromBase64String(v_Base64);
                            using MemoryStream v_Stream = new MemoryStream(v_ImageBytes);

                            MimeEntity v_Resource = await v_BodyBuilder.LinkedResources.AddAsync($"image.{v_ImageType}", v_Stream, p_CancellationToken);
                            v_Resource.ContentId = v_ContentId;
                        }
                    }
                    catch (Exception v_Ex)
                    {
                        p_Logger.LogWarning(v_Ex, "Failed to process base64 image for {ContentId}", v_ContentId);
                    }
                }
                // Handle file paths
                else if (File.Exists(v_Value))
                {
                    MimeEntity v_Resource = await v_BodyBuilder.LinkedResources.AddAsync(v_Value, p_CancellationToken);
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
