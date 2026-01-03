using Microsoft.AspNetCore.Mvc;
using RemessaSeguraBakend.DTO;
using RemessaSeguraBakend.Services;

namespace RemessaSeguraBakend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase {
    private readonly UsuarioService _service;

    public UsuariosController(UsuarioService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest req) {
        try {
            var id = await _service.CriarAsync(req);
            return CreatedAtAction(nameof(Create), new { id }, new { id });
        } catch (Exception ex) {
            return BadRequest(new { error = ex.Message });
        }
    }
}
