namespace PayOSNetCore.Models;

using MySql.Data.MySqlClient;
using PayOSNetCore.Config;
public class OrderModel
{
    private readonly MySqlConnection pool;
    public OrderModel(IConfiguration configuration)
    {
        pool = (new ConnectMySql(configuration)).Connect();
    }

    public List<Dictionary<String, dynamic>>? getOrder(int id)
    {
        try
        {
            pool.Open();
            List<Dictionary<String, dynamic>> orders = new List<Dictionary<String, dynamic>>();
            // Tạo đối tượng MySqlCommand
            MySqlCommand command = pool.CreateCommand();
            command.CommandText = "SELECT * FROM `order` WHERE id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<String, dynamic> row = new Dictionary<String, dynamic>();
                row.Add("id", reader["id"]);
                row.Add("status", reader["status"]);
                row.Add("items", reader["items"]);
                row.Add("amount", reader["amount"]);
                row.Add("ref_id", reader["ref_id"]);
                row.Add("description", reader["description"]);
                row.Add("transaction_when", reader["transaction_when"]);
                row.Add("payment_link_id", reader["payment_link_id"]);
                row.Add("transaction_code", reader["transaction_code"]);
                row.Add("created_at", reader["created_at"]);
                row.Add("updated_at", reader["updated_at"]);
                row.Add("webhook_snapshot", reader["webhook_snapshot"]);
                // Check if any value is DBNull and insert null instead
                foreach (var key in row.Keys.ToList())
                {
                    if (row[key] is DBNull)
                    {
                        row[key] = null;
                    }
                }
                orders.Add(row);
            }
            return orders;
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return null;
        }
        finally
        {
            pool.Clone();
        }
    }

    public void createOrder(int orderCode, string items, int amount, string description, string payment_link_id)
    {
        try
        {
            pool.Open();
            // Tạo đối tượng MySqlCommand
            MySqlCommand command = pool.CreateCommand();
            command.CommandText = "INSERT INTO `order` (id, items, amount, description, payment_link_id) VALUES (@OrderCode, @Items, @Amount, @Description, @PaymentLinkId)";

            // Đặt giá trị cho các tham số
            command.Parameters.AddWithValue("@OrderCode", orderCode);
            command.Parameters.AddWithValue("@Items", items);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@Description", description);
            command.Parameters.AddWithValue("@PaymentLinkId", payment_link_id);

            // Thực hiện truy vấn
            int rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                Console.WriteLine("Đã thêm đơn hàng thành công.");
            }
            else
            {
                Console.WriteLine("Không có dòng nào được thêm vào.");
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

    }
    public void updateOrderWebhook(int orderCode, string refId, string transactionCode, string webhook_snapshot)
    {
        try
        {
            pool.Open();

            // Tạo đối tượng MySqlCommand
            MySqlCommand command = pool.CreateCommand();
            command.CommandText =
            "UPDATE `order` SET ref_id = @RefId, status = 'PAID', transaction_code = @TransactionCode, webhook_snapshot = @WebhookSnapshot, transaction_when = CURRENT_TIME WHERE id = @Id";

            // Đặt giá trị cho các tham số
            command.Parameters.AddWithValue("@Id", orderCode);  // Thay đổi tên tham số thành orderId (hoặc tên thích hợp)
            command.Parameters.AddWithValue("@RefId", refId);
            command.Parameters.AddWithValue("@TransactionCode", transactionCode);
            command.Parameters.AddWithValue("@WebhookSnapshot", webhook_snapshot);

            // Thực hiện truy vấn
            int rowsAffected = command.ExecuteNonQuery();

            // Kiểm tra xem có dòng nào được cập nhật không
            if (rowsAffected > 0)
            {
                Console.WriteLine("Đã cập nhật đơn hàng thành công.");
            }
            else
            {
                Console.WriteLine("Không có dòng nào được cập nhật.");
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

    }
}