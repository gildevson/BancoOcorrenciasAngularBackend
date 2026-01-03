using Dapper;
using Npgsql;
using RemessaSeguraBakend.Models;

namespace RemessaSeguraBakend.Data {
    public class BancosRepository {
        private readonly string _cs;

        public BancosRepository(IConfiguration cfg) {
            _cs = cfg.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string não configurada");
        }

        /// <summary>
        /// Retorna todos os bancos cadastrados ordenados por número
        /// </summary>
        public async Task<List<Banco>> GetAllAsync() {
            const string sql = @"
        SELECT 
            id AS Id,
            numero_banco AS NumeroBanco,  
            nome AS Nome
        FROM bancos
        ORDER BY numero_banco;";

            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<Banco>(sql);
            return result?.ToList() ?? new List<Banco>();
        }

        /// <summary>
        /// Busca um banco específico por ID
        /// </summary>
        public async Task<Banco?> GetByIdAsync(Guid id) {
            const string sql = @"
                SELECT 
                    id AS Id,
                    numero_banco AS NumeroBanco,
                    nome AS Nome
                FROM bancos
                WHERE id = @id
                LIMIT 1;";

            try {
                await using var conn = new NpgsqlConnection(_cs);
                await conn.OpenAsync();

                return await conn.QueryFirstOrDefaultAsync<Banco>(sql, new { id });
            } catch (Exception ex) {
                throw new Exception($"Erro ao buscar banco por ID: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca um banco por número (ex: "001", "237")
        /// </summary>
        public async Task<Banco?> GetByNumeroAsync(string numeroBanco) {
            const string sql = @"
                SELECT 
                    id AS Id,
                    numero_banco AS NumeroBanco,
                    nome AS Nome
                FROM bancos
                WHERE numero_banco = @numeroBanco
                LIMIT 1;";

            try {
                await using var conn = new NpgsqlConnection(_cs);
                await conn.OpenAsync();

                return await conn.QueryFirstOrDefaultAsync<Banco>(
                    sql,
                    new { numeroBanco }
                );
            } catch (Exception ex) {
                throw new Exception($"Erro ao buscar banco por número: {ex.Message}", ex);
            }
        }


    }
}