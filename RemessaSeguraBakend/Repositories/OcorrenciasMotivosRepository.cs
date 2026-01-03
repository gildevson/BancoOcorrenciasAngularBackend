using Dapper;
using Npgsql;
using RemessaSeguraBakend.Models;

namespace RemessaSeguraBakend.Data {
    public class OcorrenciasMotivosRepository {
        private readonly string _cs;

        public OcorrenciasMotivosRepository(IConfiguration cfg) {
            _cs = cfg.GetConnectionString("DefaultConnection")!;
        }

        // ✅ NOVO: Listar todos os motivos de uma ocorrência
        public async Task<List<OcorrenciaMotivo>> GetMotivosAsync(Guid bancoId, string ocorrencia) {
            const string sql = @"
                SELECT
                    id,
                    banco_id AS BancoId,
                    ocorrencia,
                    motivo,
                    descricao,
                    observacao
                FROM ocorrencias_motivos
                WHERE banco_id = @bancoId
                  AND ocorrencia = @ocorrencia
                ORDER BY motivo;";

            await using var conn = new NpgsqlConnection(_cs);
            var result = await conn.QueryAsync<OcorrenciaMotivo>(sql, new { bancoId, ocorrencia });
            return result.ToList();
        }

        public async Task<OcorrenciaMotivo?> GetDetalheAsync(Guid bancoId, string ocorrencia, string motivo) {
            const string sql = @"
                SELECT
                    id,
                    banco_id AS BancoId,
                    ocorrencia,
                    motivo,
                    descricao,
                    observacao
                FROM ocorrencias_motivos
                WHERE banco_id = @bancoId
                  AND ocorrencia = @ocorrencia
                  AND motivo = @motivo
                LIMIT 1;";

            await using var conn = new NpgsqlConnection(_cs);
            return await conn.QueryFirstOrDefaultAsync<OcorrenciaMotivo>(
                sql, new { bancoId, ocorrencia, motivo }
            );
        }

        public async Task<OcorrenciaMotivo?> GetByIdAsync(Guid id) {
            const string sql = @"
                SELECT
                    id,
                    banco_id AS BancoId,
                    ocorrencia,
                    motivo,
                    descricao,
                    observacao
                FROM ocorrencias_motivos
                WHERE id = @id
                LIMIT 1;";

            await using var conn = new NpgsqlConnection(_cs);
            return await conn.QueryFirstOrDefaultAsync<OcorrenciaMotivo>(sql, new { id });
        }

        public async Task<Guid> CreateAsync(OcorrenciaMotivo model) {
            const string sql = @"
                INSERT INTO ocorrencias_motivos
                (id, banco_id, ocorrencia, motivo, descricao, observacao)
                VALUES
                (@Id, @BancoId, @Ocorrencia, @Motivo, @Descricao, @Observacao)
                RETURNING id;";

            await using var conn = new NpgsqlConnection(_cs);
            return await conn.ExecuteScalarAsync<Guid>(sql, model);
        }

        public async Task<int> UpdateAsync(Guid bancoId, string ocorrencia, string motivo, string descricao, string? observacao) {
            const string sql = @"
                UPDATE ocorrencias_motivos
                SET descricao = @descricao,
                    observacao = @observacao,
                    atualizado_em = NOW()
                WHERE banco_id = @bancoId
                  AND ocorrencia = @ocorrencia
                  AND motivo = @motivo;";

            await using var conn = new NpgsqlConnection(_cs);
            return await conn.ExecuteAsync(sql, new { bancoId, ocorrencia, motivo, descricao, observacao });
        }
    }
}