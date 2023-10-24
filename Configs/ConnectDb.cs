
namespace PayOSNetCore.Config;
using MySql.Data.MySqlClient;

public class ConnectMySql
{
    private readonly IConfiguration _configuration;

    public ConnectMySql(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public MySqlConnection Connect()
    {
        try
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();
            conn.ConnectionString = connectionString;
            return conn;
        }
        catch (MySql.Data.MySqlClient.MySqlException ex)
        {

            Console.WriteLine("Faild to connect", ex.Message);
            return null;
        }
    }
}