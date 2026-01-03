using RemessaSeguraBakend.DTO;
using RemessaSeguraBakend.Repositories;

namespace RemessaSeguraBakend.Services;

public class UsuarioService {
    private readonly UsuarioRepository _usuarios;
    private readonly PermissaoRepository _permissoes;

    public UsuarioService(UsuarioRepository usuarios, PermissaoRepository permissoes) {
        _usuarios = usuarios;
        _permissoes = permissoes;
    }

    public async Task<Guid> CriarAsync(CreateUsuarioRequest req) {
        if (string.IsNullOrWhiteSpace(req.Nome)) throw new Exception("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(req.Email)) throw new Exception("Email é obrigatório.");
        if (string.IsNullOrWhiteSpace(req.Senha)) throw new Exception("Senha é obrigatória.");
        if (string.IsNullOrWhiteSpace(req.Permissao)) throw new Exception("Permissão é obrigatória.");

        if (await _usuarios.ExistsByEmailAsync(req.Email))
            throw new Exception("Email já cadastrado.");

        var permissaoId = await _permissoes.GetIdByCodigoAsync(req.Permissao);
        if (permissaoId is null)
            throw new Exception("Permissão inválida. Use: ADMIN, PORTAL, SUPERVISOR.");

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Senha);

        var usuarioId = await _usuarios.CreateAsync(req.Nome, req.Email, hash);
        await _usuarios.AddPermissaoAsync(usuarioId, permissaoId.Value);

        return usuarioId;
    }
}
