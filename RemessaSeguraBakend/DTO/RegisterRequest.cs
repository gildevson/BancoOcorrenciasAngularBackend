namespace RemessaSeguraBakend.DTO;

public class RegisterRequest {
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Senha { get; set; } = "";

    // opcional: se não vier, vira PORTAL
    public string? Permissao { get; set; } // ADMIN | PORTAL | SUPERVISOR
}
