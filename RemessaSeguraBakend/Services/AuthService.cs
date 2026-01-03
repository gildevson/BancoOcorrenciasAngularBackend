using Microsoft.IdentityModel.Tokens;
using RemessaSeguraBakend.DTO;
using RemessaSeguraBakend.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RemessaSeguraBakend.Services;

public class AuthService {
    private readonly UsuarioRepository _usuarios;
    private readonly PermissaoRepository _permissoes;
    private readonly IConfiguration _cfg;

    public AuthService(UsuarioRepository usuarios, PermissaoRepository permissoes, IConfiguration cfg) {
        _usuarios = usuarios;
        _permissoes = permissoes;
        _cfg = cfg;
    }



    public async Task<LoginResponse> LoginAsync(LoginRequest req) {
        var user = await _usuarios.GetByEmailAsync(req.Email);
        if (user == null || !user.Ativo) throw new UnauthorizedAccessException("Usuário/senha inválidos.");

        if (string.IsNullOrWhiteSpace(user.SenhaHash)) throw new UnauthorizedAccessException("Usuário/senha inválidos.");

        if (!BCrypt.Net.BCrypt.Verify(req.Senha, user.SenhaHash))
            throw new UnauthorizedAccessException("Usuário/senha inválidos.");

        var roles = await _permissoes.GetCodigosByUsuarioIdAsync(user.Id);
        if (roles.Count == 0) roles.Add("PORTAL");

        var token = GerarToken(user.Id, user.Email, roles);

        return new LoginResponse {
            Token = token,
            User = new LoginUserDto {
                Id = user.Id.ToString(),
                Nome = user.Nome,
                Email = user.Email
            },
            Roles = roles
        };
    }

    private string GerarToken(Guid userId, string email, List<string> roles) {
        var key = _cfg["Jwt:Key"]!;
        var issuer = _cfg["Jwt:Issuer"]!;
        var audience = _cfg["Jwt:Audience"]!;

        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
        };

        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
