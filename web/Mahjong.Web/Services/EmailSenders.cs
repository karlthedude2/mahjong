using System.Text.Encodings.Web;
using Azure;
using Azure.Communication.Email;
using Mahjong.Web.Client;
using Mahjong.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Mahjong.Web.Services;

/// <summary>
/// The account emails Identity sends, as subject and HTML body, in the language of the page that
/// sent them (the player's language when they signed up or asked for a reset).
/// </summary>
internal sealed class AccountEmails(IStringLocalizer<Strings> L)
{
    public (string Subject, string Html) Confirmation(string link) =>
        (L["Confirm your Mahjong Haus account"],
         L["<p>Welcome to Mahjong Haus!</p><p>Please <a href='{0}'>confirm your email address</a> to finish creating your account.</p>", link]);

    public (string Subject, string Html) PasswordResetLink(string link) =>
        (L["Reset your Mahjong Haus password"],
         L["<p>You can <a href='{0}'>reset your password here</a>. If you didn't ask for this, you can ignore this email.</p>", link]);

    public (string Subject, string Html) PasswordResetCode(string code) =>
        (L["Reset your Mahjong Haus password"], L["<p>Your password reset code is <strong>{0}</strong>.</p>", HtmlEncoder.Default.Encode(code)]);
}

/// <summary>Sends account emails through Azure Communication Services.</summary>
public sealed class AcsEmailSender(EmailClient client, IOptions<EmailOptions> options, ILogger<AcsEmailSender> logger, IStringLocalizer<Strings> L)
    : IEmailSender<ApplicationUser>
{
    private readonly AccountEmails AccountEmails = new(L);

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
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger, IStringLocalizer<Strings> L) : IEmailSender<ApplicationUser>
{
    private readonly AccountEmails AccountEmails = new(L);

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
