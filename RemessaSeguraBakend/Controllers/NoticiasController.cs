using Microsoft.AspNetCore.Mvc;
using RemessaSeguraBakend.Models;
using RemessaSeguraBakend.Repositories;

namespace RemessaSeguraBakend.Controllers {
    [ApiController]
    [Route("api/noticias")]
    public class NoticiasController : ControllerBase {
        private readonly NoticiaRepository _repo;

        public NoticiasController(NoticiaRepository repo) {
            _repo = repo;
        }

        // GET /api/noticias
        [HttpGet]
        public async Task<IActionResult> Get() {
            var noticias = await _repo.GetPublicadas();
            return Ok(noticias);
        }

        // GET /api/noticias/slug/{slug}
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug) {
            var noticia = await _repo.GetBySlug(slug);
            if (noticia == null)
                return NotFound(new { message = "Notícia não encontrada." });

            return Ok(noticia);
        }

        // GET /api/noticias/categoria/{categoria}
        [HttpGet("categoria/{categoria}")]
        public async Task<IActionResult> GetByCategoria(string categoria) {
            var noticias = await _repo.GetByCategoria(categoria);
            return Ok(noticias);
        }

        // GET /api/noticias/destaques
        [HttpGet("destaques")]
        public async Task<IActionResult> GetDestaques([FromQuery] int limit = 5) {
            var noticias = await _repo.GetDestaques(limit);
            return Ok(noticias);
        }

        // GET /api/noticias/mais-lidas
        [HttpGet("mais-lidas")]
        public async Task<IActionResult> GetMaisLidas([FromQuery] int limit = 10) {
            var noticias = await _repo.GetMaisLidas(limit);
            return Ok(noticias);
        }

        // GET /api/noticias/admin/all
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAll() {
            var noticias = await _repo.GetAll();
            return Ok(noticias);
        }

        // POST /api/noticias
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Noticia noticia) {
            try {
                var created = await _repo.Create(noticia);
                return CreatedAtAction(
                    nameof(GetBySlug),
                    new { slug = created.Slug },
                    created
                );
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/noticias/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Noticia noticia) {
            try {
                var updated = await _repo.Update(id, noticia);
                if (updated == null)
                    return NotFound(new { message = "Notícia não encontrada." });

                return Ok(updated);
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/noticias/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id) {
            var deleted = await _repo.Delete(id);
            if (!deleted)
                return NotFound(new { message = "Notícia não encontrada." });

            return NoContent();
        }

        // POST /api/noticias/{id}/visualizar
        [HttpPost("{id}/visualizar")]
        public async Task<IActionResult> Visualizar(Guid id) {
            await _repo.IncrementarVisualizacoes(id);
            return Ok(new { message = "Visualização registrada." });
        }
    }
}