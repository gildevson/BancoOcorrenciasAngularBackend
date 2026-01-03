using Microsoft.AspNetCore.Mvc;
using RemessaSeguraBakend.Data;
using RemessaSeguraBakend.DTO;
using RemessaSeguraBakend.Models;
using Npgsql;

namespace RemessaSeguraBakend.Controllers {
    [ApiController]
    [Route("api/bancos/{bancoId:guid}/ocorrencias/{ocorrencia}/motivos")]
    public class OcorrenciasMotivosController : ControllerBase {
        private readonly OcorrenciasMotivosRepository _repo;

        public OcorrenciasMotivosController(OcorrenciasMotivosRepository repo) {
            _repo = repo;
        }

        // ✅ NOVO: GET /api/bancos/{bancoId}/ocorrencias/{ocorrencia}/motivos
        [HttpGet]
        public async Task<ActionResult<List<OcorrenciaMotivo>>> GetMotivos(
            [FromRoute] Guid bancoId,
            [FromRoute] string ocorrencia
        ) {
            var motivos = await _repo.GetMotivosAsync(bancoId, ocorrencia);
            return Ok(motivos);
        }

        // GET /api/bancos/{bancoId}/ocorrencias/{ocorrencia}/motivos/{motivo}
        [HttpGet("{motivo}")]
        public async Task<ActionResult<OcorrenciaMotivo>> GetDetalhe(
            [FromRoute] Guid bancoId,
            [FromRoute] string ocorrencia,
            [FromRoute] string motivo
        ) {
            var item = await _repo.GetDetalheAsync(bancoId, ocorrencia, motivo);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST /api/bancos/ocorrencias/motivos
        [HttpPost]
        [Route("/api/bancos/ocorrencias/motivos")]
        public async Task<IActionResult> CreateSimples(
            [FromBody] CreateOcorrenciaMotivoRequest req
        ) {
            if (req.BancoId == Guid.Empty) return BadRequest("bancoId é obrigatório.");
            if (string.IsNullOrWhiteSpace(req.Ocorrencia)) return BadRequest("ocorrencia é obrigatória.");
            if (string.IsNullOrWhiteSpace(req.Motivo)) return BadRequest("motivo é obrigatório.");

            var model = new OcorrenciaMotivo {
                Id = Guid.NewGuid(),
                BancoId = req.BancoId,
                Ocorrencia = req.Ocorrencia.Trim(),
                Motivo = req.Motivo.Trim(),
                Descricao = req.Descricao?.Trim() ?? "",
                Observacao = string.IsNullOrWhiteSpace(req.Observacao) ? null : req.Observacao.Trim()
            };

            try {
                await _repo.CreateAsync(model);
                return CreatedAtAction(
                    nameof(GetDetalhe),
                    new { bancoId = model.BancoId, ocorrencia = model.Ocorrencia, motivo = model.Motivo },
                    model
                );
            } catch (PostgresException ex) when (ex.SqlState == "23505") {
                return Conflict("Já existe esse motivo para esse banco/ocorrência.");
            }
        }

        // PUT /api/bancos/{bancoId}/ocorrencias/{ocorrencia}/motivos/{motivo}
        [HttpPut("{motivo}")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid bancoId,
            [FromRoute] string ocorrencia,
            [FromRoute] string motivo,
            [FromBody] UpdateOcorrenciaMotivoRequest req
        ) {
            var atual = await _repo.GetDetalheAsync(bancoId, ocorrencia, motivo);
            if (atual == null) return NotFound();

            var descricaoFinal = req.Descricao ?? atual.Descricao;
            var observacaoFinal = req.Observacao ?? atual.Observacao;

            var rows = await _repo.UpdateAsync(bancoId, ocorrencia, motivo, descricaoFinal, observacaoFinal);
            if (rows == 0) return NotFound();

            return NoContent();
        }
    }
}