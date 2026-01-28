using MySqlConnector;

namespace MiniErp.Api.Data;

public class Db
{
    private readonly IConfiguration _config;
    public Db(IConfiguration config) => _config = config;
    public MySqlConnection CreateConnection() => new(_config.GetConnectionString("MySql"));
}
