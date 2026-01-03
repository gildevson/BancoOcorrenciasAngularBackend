using Dapper;
using RemessaSeguraBakend.Data;

namespace RemessaSeguraBakend.Repositories;

public class PermissaoRepository {
    private readonly DbConnectionFactory _factory;
    public PermissaoRepository(DbConnectionFactory factory) => _factory = factory;

    public async Task<Guid?> GetIdByCodigoAsync(string codigo) {
        var c = codigo.Trim().ToUpperInvariant();
        using var conn = _factory.CreateConnection();
        return await conn.ExecuteScalarAsync<Guid?>(@"
            SELECT id FROM permissoes WHERE codigo=@codigo LIMIT 1;
        ", new { codigo = c });
    }

    public async Task<List<string>> GetCodigosByUsuarioIdAsync(Guid usuarioId) {
        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<string>(@"
            SELECT p.codigo
            FROM usuario_permissoes up
            JOIN permissoes p ON p.id = up.permissao_id
            WHERE up.usuario_id = @usuarioId
            ORDER BY p.codigo;
        ", new { usuarioId });

        return rows.ToList();
    }
}
