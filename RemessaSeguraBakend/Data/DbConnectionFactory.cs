using Npgsql;
using System.Data;

namespace RemessaSeguraBakend.Data {
    public class DbConnectionFactory {
        private readonly IConfiguration _config;

        public DbConnectionFactory(IConfiguration config) {
            _config = config;
        }

        public IDbConnection CreateConnection() {
            var cs = _config.GetConnectionString("DefaultConnection");
            return new NpgsqlConnection(cs);
        }
    }
}
