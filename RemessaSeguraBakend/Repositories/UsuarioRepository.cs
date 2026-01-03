using Dapper;
using RemessaSeguraBakend.Data;

namespace RemessaSeguraBakend.Repositories;

public class UsuarioRepository {
    private readonly DbConnectionFactory _factory;
    public UsuarioRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<bool> ExistsByEmailAsync(string email) {
        var normalized = email.Trim().ToLowerInvariant();
        using var conn = _factory.CreateConnection();
        var qtd = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM usuarios WHERE lower(email)=@email",
            new { email = normalized }
        );
        return qtd > 0;
    }

    public async Task<Guid> CreateAsync(string nome, string email, string senhaHash) {
        var normalized = email.Trim().ToLowerInvariant();
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<Guid>(@"
            INSERT INTO usuarios (nome, email, senha_hash)
            VALUES (@nome, @email, @senha_hash)
            RETURNING id;
        ", new { nome, email = normalized, senha_hash = senhaHash });
    }

    public async Task AddPermissaoAsync(Guid usuarioId, Guid permissaoId) {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(@"
            INSERT INTO usuario_permissoes (usuario_id, permissao_id)
            VALUES (@usuarioId, @permissaoId)
            ON CONFLICT DO NOTHING;
        ", new { usuarioId, permissaoId });
    }

    // usado no login
    public async Task<UsuarioLoginView?> GetByEmailAsync(string email) {
        var normalized = email.Trim().ToLowerInvariant();
        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<UsuarioLoginView>(@"
            SELECT
              id,
              nome,
              email,
              senha_hash as SenhaHash,
              ativo
            FROM usuarios
            WHERE lower(email)=@email
            LIMIT 1;
        ", new { email = normalized });
    }

    public async Task<Guid?> GetIdByEmailAsync(string email) {
        var normalized = email.Trim().ToLowerInvariant();
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<Guid?>(@"
        SELECT id
        FROM usuarios
        WHERE lower(email)=@email
        LIMIT 1;
    ", new { email = normalized });
    }

    public async Task AtualizarSenhaHashAsync(Guid userId, string senhaHash) {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(@"
        UPDATE usuarios
        SET senha_hash = @senha_hash
        WHERE id = @id;
    ", new { id = userId, senha_hash = senhaHash });
    }



}



public class UsuarioLoginView {
    public Guid Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public bool Ativo { get; set; }
}
