using Microsoft.AspNetCore.Mvc;
using RemessaSeguraBakend.Data; // ou RemessaSeguraBakend.Repositories
using RemessaSeguraBakend.Models;
using RemessaSeguraBakend.Repositories;

namespace RemessaSeguraBakend.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class BancosController : ControllerBase {
        private readonly BancoRepository _repo; // ✅ Use BancoRepository ao invés de BancosRepository

        public BancosController(BancoRepository repo) {
            _repo = repo;
        }

        // GET /api/bancos
        [HttpGet]
        public async Task<ActionResult<List<Banco>>> GetAll() {
            try {
                var bancos = await _repo.GetAllAsync();
                return Ok(bancos);
            } catch (Exception ex) {
                return StatusCode(500, $"Erro ao buscar bancos: {ex.Message}");
            }
        }
    }
}