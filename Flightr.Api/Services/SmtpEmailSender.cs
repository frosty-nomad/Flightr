using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Flightr.Api.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation("SMTP host not configured; skipping sending email to {Email}. Subject: {Subject}", to, subject);
            _logger.LogInformation("Email body: {Body}", htmlBody);
            return;
        }

        var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 25;
        var user = _configuration["Smtp:Username"];
        var pass = _configuration["Smtp:Password"];
        var from = _configuration["Smtp:From"] ?? _configuration["Smtp:Username"] ?? "no-reply@flightr.local";

        _logger.LogInformation("SMTP Configuration: Host={Host}, Port={Port}, User={User}, SSL={SSL}", host, port, user, bool.TryParse(_configuration["Smtp:EnableSsl"], out var enableSsl) && enableSsl);
        _logger.LogInformation("Password is {Status}", string.IsNullOrWhiteSpace(pass) ? "NOT SET" : "SET");

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = bool.TryParse(_configuration["Smtp:EnableSsl"], out var ssl) && ssl
        };

        if (!string.IsNullOrWhiteSpace(user))
        {
            client.Credentials = new NetworkCredential(user, pass);
            _logger.LogInformation("SMTP credentials set for user {User}", user);
        }
        else
        {
            _logger.LogWarning("SMTP username is not configured; sending without authentication");
        }

        using var msg = new MailMessage(from, to, subject, htmlBody)
        {
            IsBodyHtml = true
        };

        try
        {
            await client.SendMailAsync(msg);
            _logger.LogInformation("Sent email to {Email} via SMTP host {Host}", to, host);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }
}
