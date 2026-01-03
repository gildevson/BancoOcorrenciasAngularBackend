using System.Security.Cryptography;
using System.Text;
using RemessaSeguraBakend.Repositories;

namespace RemessaSeguraBakend.Services;

public class PasswordResetService {
    private readonly UsuarioRepository _usuarios;
    private readonly ResetSenhaRepository _resetRepo;
    private readonly EmailService _email;
    private readonly IConfiguration _cfg;

    public PasswordResetService(
        UsuarioRepository usuarios,
        ResetSenhaRepository resetRepo,
        EmailService email,
        IConfiguration cfg
    ) {
        _usuarios = usuarios;
        _resetRepo = resetRepo;
        _email = email;
        _cfg = cfg;
    }

    public async Task SolicitarAsync(string email) {
        var userId = await _usuarios.GetIdByEmailAsync(email);
        if (userId == null) return;

        var token = GerarTokenSeguro();
        var tokenHash = Sha256(token);
        var expiraEm = DateTime.UtcNow.AddMinutes(30);

        await _resetRepo.CriarTokenAsync(userId.Value, tokenHash, expiraEm);

        var baseUrl = _cfg["App:FrontendBaseUrl"] ?? "http://localhost:4200";

        // ✅ CORRIGIDO: /reset-senha → /reset-password
        var link = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";

        var html = $@"
<div style='font-family:Arial'>
  <h2>Redefinição de senha</h2>
  <p>Clique no botão abaixo:</p>
  <p><a href='{link}' style='background:#4a55ff;color:#fff;padding:10px 14px;border-radius:8px;text-decoration:none'>Redefinir senha</a></p>
  <p>Ou copie e cole:</p>
  <p>{link}</p>
  <p><small>Expira em 30 minutos.</small></p>
</div>";

        await _email.EnviarAsync(email.Trim(), "Redefinição de senha - Remessa Segura", html);
    }

    public async Task RedefinirAsync(string token, string novaSenha) {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(novaSenha))
            throw new ArgumentException("Token e nova senha são obrigatórios.");

        if (novaSenha.Length < 6)
            throw new ArgumentException("A senha deve ter no mínimo 6 caracteres.");

        var tokenHash = Sha256(token);
        var userId = await _resetRepo.ObterUsuarioPorTokenValidoAsync(tokenHash);
        if (userId == null)
            throw new UnauthorizedAccessException("Token inválido ou expirado.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
        await _usuarios.AtualizarSenhaHashAsync(userId.Value, senhaHash);

        await _resetRepo.MarcarComoUsadoAsync(tokenHash);
    }

    private static string GerarTokenSeguro() {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var b64 = Convert.ToBase64String(bytes);
        return b64.Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private static string Sha256(string input) {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
