using System.Text.Encodings.Web;
using Azure;
using Azure.Communication.Email;
using Mahjong.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Mahjong.Web.Services;

/// <summary>The account emails Identity sends, as subject and HTML body.</summary>
internal static class AccountEmails
{
    public static (string Subject, string Html) Confirmation(string link) =>
        ("Confirm your Mahjong account",
         $"<p>Welcome to Mahjong!</p><p>Please <a href='{link}'>confirm your email address</a> to finish creating your account.</p>");

    public static (string Subject, string Html) PasswordResetLink(string link) =>
        ("Reset your Mahjong password",
         $"<p>You can <a href='{link}'>reset your password here</a>. If you didn't ask for this, you can ignore this email.</p>");

    public static (string Subject, string Html) PasswordResetCode(string code) =>
        ("Reset your Mahjong password", $"<p>Your password reset code is <strong>{HtmlEncoder.Default.Encode(code)}</strong>.</p>");
}

/// <summary>Sends account emails through Azure Communication Services.</summary>
public sealed class AcsEmailSender(EmailClient client, IOptions<EmailOptions> options, ILogger<AcsEmailSender> logger)
    : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendAsync(email, AccountEmails.Confirmation(confirmationLink));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendAsync(email, AccountEmails.PasswordResetLink(resetLink));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendAsync(email, AccountEmails.PasswordResetCode(resetCode));

    private async Task SendAsync(string to, (string Subject, string Html) message)
    {
        try
        {
            // Don't wait for delivery; ACS accepts the message and delivers it in the background.
            await client.SendAsync(WaitUntil.Started, options.Value.SenderAddress, to, message.Subject, message.Html);
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Failed to send \"{Subject}\" email", message.Subject);
            throw;
        }
    }
}

/// <summary>Development sender: writes account emails (with their links) to the log.</summary>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        Log(email, AccountEmails.Confirmation(confirmationLink));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        Log(email, AccountEmails.PasswordResetLink(resetLink));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        Log(email, AccountEmails.PasswordResetCode(resetCode));

    private Task Log(string to, (string Subject, string Html) message)
    {
        logger.LogWarning("Email not sent (no Email:ConnectionString). To {To}: {Subject}\n{Html}", to, message.Subject, message.Html);
        return Task.CompletedTask;
    }
}
