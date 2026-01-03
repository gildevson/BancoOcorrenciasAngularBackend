using Dapper;
using Microsoft.AspNetCore.Mvc;
using RemessaSeguraBakend.Data;

namespace RemessaSeguraBakend.Controllers {
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase {
        private readonly DbConnectionFactory _factory;

        public HealthController(DbConnectionFactory factory) {
            _factory = factory;
        }

        [HttpGet("db")]
        public IActionResult TestDatabase() {
            try {
                using var conn = _factory.CreateConnection();
                var result = conn.ExecuteScalar<int>("SELECT 1");

                return Ok(new {
                    status = "OK",
                    result,
                    message = "Conexão com Neon PostgreSQL realizada com sucesso 🚀"
                });
            } catch (Exception ex) {
                return StatusCode(500, new {
                    status = "ERROR",
                    error = ex.Message
                });
            }
        }
    }
}
