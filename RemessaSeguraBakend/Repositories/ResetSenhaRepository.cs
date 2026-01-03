using Dapper;
using RemessaSeguraBakend.Data;

namespace RemessaSeguraBakend.Repositories;

public class ResetSenhaRepository {
    private readonly DbConnectionFactory _factory;
    public ResetSenhaRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task CriarTokenAsync(Guid usuarioId, string tokenHash, DateTime expiraEmUtc) {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(@"
        INSERT INTO reset_senha_tokens (usuario_id, token_hash, expira_em)
        VALUES (@usuarioId, @tokenHash, @expiraEm);
    ", new { usuarioId, tokenHash, expiraEm = expiraEmUtc });
    }

    public async Task<Guid?> ObterUsuarioPorTokenValidoAsync(string tokenHash) {
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<Guid?>(@"
        SELECT usuario_id
        FROM reset_senha_tokens
        WHERE token_hash = @tokenHash
          AND used_at IS NULL
          AND expira_em > now()
        ORDER BY created_at DESC
        LIMIT 1;
    ", new { tokenHash });
    }

    public async Task MarcarComoUsadoAsync(string tokenHash) {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(@"
        UPDATE reset_senha_tokens
        SET used_at = now()
        WHERE token_hash = @tokenHash
          AND used_at IS NULL;
    ", new { tokenHash });
    }
}
