using Dapper;
using RemessaSeguraBakend.Data;
using RemessaSeguraBakend.Models;

namespace RemessaSeguraBakend.Repositories {
    public class NoticiaRepository {
        private readonly DbConnectionFactory _factory;

        public NoticiaRepository(DbConnectionFactory factory) {
            _factory = factory;
        }

        private const string BaseSelect = @"
            SELECT
                id, titulo, slug, resumo, conteudo,
                imagem_capa AS ImagemCapa,
                categoria, status,
                data_publicacao AS DataPublicacao,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt,
                publicado,
                autor_nome AS AutorNome,
                visualizacoes,
                destaque,
                meta_description AS MetaDescription,
                ordem_destaque AS OrdemDestaque,
                fonte_nome AS FonteNome,
                fonte_url AS FonteUrl,
                fonte_publicada_em AS FontePublicadaEm,
                fonte_autor AS FonteAutor
            FROM noticias
        ";

        public async Task<IEnumerable<Noticia>> GetPublicadas() {
            using var conn = _factory.CreateConnection();
            var sql = BaseSelect + @"
                WHERE publicado = true
                ORDER BY data_publicacao DESC NULLS LAST, created_at DESC
            ";
            return await conn.QueryAsync<Noticia>(sql);
        }

        public async Task<Noticia?> GetBySlug(string slug) {
            using var conn = _factory.CreateConnection();
            var sql = BaseSelect + @"
                WHERE slug = @Slug AND publicado = true
                LIMIT 1
            ";
            return await conn.QuerySingleOrDefaultAsync<Noticia>(sql, new { Slug = slug });
        }

        public async Task<IEnumerable<Noticia>> GetAll() {
            using var conn = _factory.CreateConnection();
            var sql = BaseSelect + @" ORDER BY created_at DESC ";
            return await conn.QueryAsync<Noticia>(sql);
        }

        public async Task<IEnumerable<Noticia>> GetByCategoria(string categoria) {
            using var conn = _factory.CreateConnection();
            var sql = BaseSelect + @"
                WHERE categoria = @Categoria AND publicado = true
                ORDER BY data_publicacao DESC NULLS LAST, created_at DESC
            ";
            return await conn.QueryAsync<Noticia>(sql, new { Categoria = categoria });
        }

        public async Task<IEnumerable<Noticia>> GetDestaques(int limit = 5) {
            using var conn = _factory.CreateConnection();
            var sql = BaseSelect + @"
                WHERE publicado = true AND destaque = true
                ORDER BY ordem_destaque ASC NULLS LAST, data_publicacao DESC
                LIMIT @Limit
            ";
            return await conn.QueryAsync<Noticia>(sql, new { Limit = limit });
        }

        public async Task<IEnumerable<Noticia>> GetMaisLidas(int limit = 10) {
            using var conn = _factory.CreateConnection();
            var sql = BaseSelect + @"
                WHERE publicado = true
                ORDER BY visualizacoes DESC, data_publicacao DESC
                LIMIT @Limit
            ";
            return await conn.QueryAsync<Noticia>(sql, new { Limit = limit });
        }

        public async Task IncrementarVisualizacoes(Guid id) {
            using var conn = _factory.CreateConnection();
            await conn.ExecuteAsync(
                "UPDATE noticias SET visualizacoes = COALESCE(visualizacoes, 0) + 1 WHERE id = @Id",
                new { Id = id }
            );
        }

        public async Task<Noticia> Create(Noticia noticia) {
            using var conn = _factory.CreateConnection();

            noticia.Id = Guid.NewGuid();
            noticia.CreatedAt = DateTime.UtcNow;
            noticia.Visualizacoes = 0;

            var sql = @"
                INSERT INTO noticias (
                    id, titulo, slug, resumo, conteudo,
                    imagem_capa, categoria, status,
                    data_publicacao, created_at, publicado,
                    autor_nome, visualizacoes,
                    destaque, meta_description, ordem_destaque,
                    fonte_nome, fonte_url, fonte_publicada_em, fonte_autor
                )
                VALUES (
                    @Id, @Titulo, @Slug, @Resumo, @Conteudo,
                    @ImagemCapa, @Categoria, @Status,
                    @DataPublicacao, @CreatedAt, @Publicado,
                    @AutorNome, @Visualizacoes,
                    @Destaque, @MetaDescription, @OrdemDestaque,
                    @FonteNome, @FonteUrl, @FontePublicadaEm, @FonteAutor
                )
                RETURNING
                    id, titulo, slug, resumo, conteudo,
                    imagem_capa AS ImagemCapa,
                    categoria, status,
                    data_publicacao AS DataPublicacao,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt,
                    publicado,
                    autor_nome AS AutorNome,
                    visualizacoes,
                    destaque,
                    meta_description AS MetaDescription,
                    ordem_destaque AS OrdemDestaque,
                    fonte_nome AS FonteNome,
                    fonte_url AS FonteUrl,
                    fonte_publicada_em AS FontePublicadaEm,
                    fonte_autor AS FonteAutor
            ";

            return await conn.QuerySingleAsync<Noticia>(sql, noticia);
        }

        public async Task<Noticia?> Update(Guid id, Noticia noticia) {
            using var conn = _factory.CreateConnection();

            noticia.Id = id;
            noticia.UpdatedAt = DateTime.UtcNow;

            var sql = @"
                UPDATE noticias
                SET titulo = @Titulo,
                    slug = @Slug,
                    resumo = @Resumo,
                    conteudo = @Conteudo,
                    imagem_capa = @ImagemCapa,
                    categoria = @Categoria,
                    status = @Status,
                    data_publicacao = @DataPublicacao,
                    updated_at = @UpdatedAt,
                    publicado = @Publicado,
                    autor_nome = @AutorNome,
                    destaque = @Destaque,
                    meta_description = @MetaDescription,
                    ordem_destaque = @OrdemDestaque,
                    fonte_nome = @FonteNome,
                    fonte_url = @FonteUrl,
                    fonte_publicada_em = @FontePublicadaEm,
                    fonte_autor = @FonteAutor
                WHERE id = @Id
                RETURNING
                    id, titulo, slug, resumo, conteudo,
                    imagem_capa AS ImagemCapa,
                    categoria, status,
                    data_publicacao AS DataPublicacao,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt,
                    publicado,
                    autor_nome AS AutorNome,
                    visualizacoes,
                    destaque,
                    meta_description AS MetaDescription,
                    ordem_destaque AS OrdemDestaque,
                    fonte_nome AS FonteNome,
                    fonte_url AS FonteUrl,
                    fonte_publicada_em AS FontePublicadaEm,
                    fonte_autor AS FonteAutor
            ";

            return await conn.QuerySingleOrDefaultAsync<Noticia>(sql, noticia);
        }

        public async Task<bool> Delete(Guid id) {
            using var conn = _factory.CreateConnection();
            var affected = await conn.ExecuteAsync("DELETE FROM noticias WHERE id = @Id", new { Id = id });
            return affected > 0;
        }

        public async Task<Noticia?> GetById(Guid id) {
            using var conn = _factory.CreateConnection();
            var sql = BaseSelect + @" WHERE id = @Id LIMIT 1";
            return await conn.QuerySingleOrDefaultAsync<Noticia>(sql, new { Id = id });
        }
    }
}
