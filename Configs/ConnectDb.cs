
namespace PayOSNetCore.Config;
using MySql.Data.MySqlClient;

public class ConnectMySql
{
    private readonly string? connectionString;

    public ConnectMySql(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public MySqlConnection? Connect()
    {
        try
        {
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