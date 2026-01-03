namespace RemessaSeguraBakend.DTO;

public class ResetPasswordRequest {
    public string Token { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}