using Microsoft.AspNetCore.Mvc;
using RemessaSeguraBakend.DTO;
using RemessaSeguraBakend.Services;

namespace RemessaSeguraBakend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase {
    private readonly AuthService _auth;
    private readonly PasswordResetService _reset;

    public AuthController(AuthService auth, PasswordResetService reset) {
        _auth = auth;
        _reset = reset;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req) {
        var res = await _auth.LoginAsync(req);
        return Ok(res);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req) {
        await _reset.SolicitarAsync(req.Email);
        return Ok(new { message = "Se o e-mail existir, enviaremos instruções para redefinir a senha." });
    }

    [HttpGet("test-reset")]
    public IActionResult TestReset() {
        return Ok(new { message = "Endpoint reset-password existe e está funcionando!" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req) {
        try {
            // Log para debug
            Console.WriteLine($"Token recebido: {req.Token}");
            Console.WriteLine($"Nova senha recebida: {req.NovaSenha}");

            await _reset.RedefinirAsync(req.Token, req.NovaSenha);
            return Ok(new { message = "Senha redefinida com sucesso!" });
        } catch (UnauthorizedAccessException ex) {
            Console.WriteLine($"Erro de autenticação: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        } catch (ArgumentException ex) {
            Console.WriteLine($"Erro de validação: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        } catch (Exception ex) {
            Console.WriteLine($"Erro inesperado: {ex.Message}");
            return StatusCode(500, new { message = "Erro interno do servidor." });
        }
    }


}
