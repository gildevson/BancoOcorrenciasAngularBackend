using MailKit.Net.Smtp;
using MimeKit;

namespace RemessaSeguraBakend.Services;

public class EmailService {
    private readonly IConfiguration _cfg;

    public EmailService(IConfiguration cfg) => _cfg = cfg;

    public async Task EnviarAsync(string to, string subject, string html) {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _cfg["Smtp:FromName"] ?? "Remessa Segura",
            _cfg["Smtp:From"] ?? _cfg["Smtp:User"]!
        ));

        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = html };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _cfg["Smtp:Host"]!,
            int.Parse(_cfg["Smtp:Port"]!),
            useSsl: true  // SSL direto na porta 465
        );

        await client.AuthenticateAsync(
            _cfg["Smtp:User"]!,
            _cfg["Smtp:Pass"]!
        );

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}