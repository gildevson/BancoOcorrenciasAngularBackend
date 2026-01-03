namespace RemessaSeguraBakend.DTO;

public class LoginResponse {
    public string Token { get; set; } = "";
    public LoginUserDto User { get; set; } = new();
    public List<string> Roles { get; set; } = new();
}
