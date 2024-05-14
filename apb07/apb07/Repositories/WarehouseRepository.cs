using System.Data;
using System.Data.SqlClient;
using apb07.Models.DTOs;

namespace apb07.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly IConfiguration _configuration;

        public WarehouseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task<SqlConnection> OpenConnectionAsync()
        {
            var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await connection.OpenAsync();
            return connection;
        }

        private async Task<bool> RecordExistsAsync(string query, SqlParameter[] parameters)
        {
            using var connection = await OpenConnectionAsync();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result != null;
        }

        public async Task<bool> DoesProductExist(int id)
        {
            var query = "SELECT 1 FROM Product WHERE IdProduct = @ID";
            var parameters = new[] { new SqlParameter("@ID", id) };
            return await RecordExistsAsync(query, parameters);
        }

        public async Task<bool> DoesWarehouseExist(int id)
        {
            var query = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @ID";
            var parameters = new[] { new SqlParameter("@ID", id) };
            return await RecordExistsAsync(query, parameters);
        }

        public async Task<bool> DoesOrderExist(int id, int amount, DateTime createdAt)
        {
            var query = "SELECT 1 FROM [Order] WHERE Amount = @Amount AND IdProduct = @ID AND CreatedAt < @CreatedAt";
            var parameters = new[]
            {
                new SqlParameter("@ID", id),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@CreatedAt", createdAt)
            };
            return await RecordExistsAsync(query, parameters);
        }

        public async Task<bool> DoesOrderCompleted(int id, int amount, DateTime createdAt)
        {
            var query = "SELECT 1 FROM Product_Warehouse JOIN [Order] O on Product_Warehouse.IdOrder = O.IdOrder WHERE O.Amount = @Amount AND O.IdProduct = @ID AND O.CreatedAt < @CreatedAt";
            var parameters = new[]
            {
                new SqlParameter("@ID", id),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@CreatedAt", createdAt)
            };
            return await RecordExistsAsync(query, parameters);
        }

        public async Task UpdateOrder(int id, int amount, DateTime createdAt)
        {
            var query = "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdProduct = @ID AND Amount = @Amount AND CreatedAt = @CreatedAt";
            using var connection = await OpenConnectionAsync();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ID", id);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            command.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertToProductWarehouse(WarehouseDTO warehouseDto)
        {
            var insert = @"INSERT INTO Product_Warehouse VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                           SELECT SCOPE_IDENTITY();";
            using var connection = await OpenConnectionAsync();
            using var command = new SqlCommand(insert, connection);
            var orderId = await GetOrderId(warehouseDto.IdProduct, warehouseDto.Amount, warehouseDto.CreatedAt);
            var productPrice = await GetProductPrice(warehouseDto.IdProduct);

            command.Parameters.AddWithValue("@IdWarehouse", warehouseDto.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            command.Parameters.AddWithValue("@Amount", warehouseDto.Amount);
            command.Parameters.AddWithValue("@Price", warehouseDto.Amount * productPrice);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            var id = await command.ExecuteScalarAsync();
            return id == DBNull.Value ? throw new Exception() : Convert.ToInt32(id);
        }

        public async Task<int> GetOrderId(int id, int amount, DateTime createdAt)
        {
            var query = "SELECT IdOrder FROM [Order] WHERE Amount = @Amount AND IdProduct = @ID AND CreatedAt < @CreatedAt";
            var parameters = new[]
            {
                new SqlParameter("@ID", id),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@CreatedAt", createdAt)
            };
            using var connection = await OpenConnectionAsync();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters);

            var orderId = await command.ExecuteScalarAsync();
            return orderId == DBNull.Value ? 0 : Convert.ToInt32(orderId);
        }

        public async Task<double> GetProductPrice(int id)
        {
            var query = "SELECT Price FROM Product WHERE ID = @ID";
            var parameters = new[] { new SqlParameter("@ID", id) };
            using var connection = await OpenConnectionAsync();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddRange(parameters);

            var price = await command.ExecuteScalarAsync();
            return price == DBNull.Value ? 0.0 : Convert.ToDouble(price);
        }

        public async Task<int> ExecuteProcedure(WarehouseDTO warehouseDto)
        {
            using var connection = await OpenConnectionAsync();
            using var command = new SqlCommand("AddProductToWarehouse", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@IdProduct", warehouseDto.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", warehouseDto.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", warehouseDto.Amount);
            command.Parameters.AddWithValue("@CreatedAt", warehouseDto.CreatedAt);

            var id = await command.ExecuteScalarAsync();
            return id == DBNull.Value ? 0 : Convert.ToInt32(id);
        }
    }
}
