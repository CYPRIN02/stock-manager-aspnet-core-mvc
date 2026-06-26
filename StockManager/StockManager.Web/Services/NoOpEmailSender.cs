using Microsoft.AspNetCore.Identity.UI.Services;

namespace StockManager.Web.Services;

public class NoOpEmailSender : IEmailSender
{
    private readonly ILogger<NoOpEmailSender> _logger;

    public NoOpEmailSender(ILogger<NoOpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogWarning(
            "Email non envoyé car NoOpEmailSender est actif | Destinataire={Email} | Sujet={Subject}",
            email,
            subject);

        return Task.CompletedTask;
    }
}
