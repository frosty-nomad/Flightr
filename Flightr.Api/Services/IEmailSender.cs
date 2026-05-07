using System.Threading.Tasks;

namespace Flightr.Api.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}
