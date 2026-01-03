namespace RemessaSeguraBakend.DTO;

public class CreateUsuarioRequest {
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Senha { get; set; } = "";
    public string Permissao { get; set; } = ""; // ADMIN | PORTAL | SUPERVISOR
}
