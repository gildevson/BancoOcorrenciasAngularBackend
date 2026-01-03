using Dapper;
using RemessaSeguraBakend.Data;

namespace RemessaSeguraBakend.Repositories {
    public class BancoRepository {
        private readonly DbConnectionFactory _db;

        public BancoRepository(DbConnectionFactory db) {
            _db = db;
        }

        public async Task<IEnumerable<dynamic>> GetAllAsync() {
            const string sql = @"SELECT id, numero_banco, nome FROM bancos ORDER BY numero_banco;";
            using var conn = _db.CreateConnection();
            return await conn.QueryAsync(sql);
        }

        public async Task<Guid?> GetIdByNumeroAsync(int numeroBanco) {
            const string sql = @"SELECT id FROM bancos WHERE numero_banco = @numeroBanco LIMIT 1;";
            using var conn = _db.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Guid?>(sql, new { numeroBanco });
        }

        public async Task<int> DeleteAsync(Guid id) {
            const string sql = @"DELETE FROM bancos WHERE id = @id;";
            using var conn = _db.CreateConnection();
            return await conn.ExecuteAsync(sql, new { id });
        }
    }
}
