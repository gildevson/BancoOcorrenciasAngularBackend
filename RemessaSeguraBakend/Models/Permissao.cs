namespace RemessaSeguraBakend.Models;

public class Permissao {
    public Guid Id { get; set; }
    public string Codigo { get; set; } = ""; // ADMIN/PORTAL/SUPERVISOR
    public string Descricao { get; set; } = "";
}
