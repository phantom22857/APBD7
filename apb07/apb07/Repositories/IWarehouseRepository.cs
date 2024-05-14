using apb07.Models.DTOs;

namespace apb07.Repositories;

public interface IWarehouseRepository
{
    Task<bool> DoesProductExist(int id);
    Task<bool> DoesWarehouseExist(int id);
    Task<bool> DoesOrderExist(int id, int amount, DateTime createdAt);
    Task<bool> DoesOrderCompleted(int id, int amount, DateTime createdAt);
    Task UpdateOrder(int id, int amount, DateTime createdAt);
    Task<int> InsertToProductWarehouse(WarehouseDTO warehouseDto);
    Task<int> GetOrderId(int id, int amount, DateTime createdAt);
    Task<double> GetProductPrice(int id);
    Task<int> ExecuteProcedure(WarehouseDTO warehouseDto);
}