using apb07.Models.DTOs;
using apb07.Repositories;
using apb07.Service;
using Microsoft.AspNetCore.Mvc;

namespace apb07.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private IWarehouseRepository _repository;
    private WarehouseService _service;

    public WarehouseController(IWarehouseRepository repository)
    {
        _repository = repository;
        _service = new WarehouseService();
    }

    [HttpPost]
    [Route("task1")]
    public async Task<IActionResult> Task1(WarehouseDTO dto)
    {
        if (!await _repository.DoesProductExist(dto.IdProduct))
            return NotFound($"Product with given ID - {dto.IdProduct} doesn't exist");
        if (!await _repository.DoesWarehouseExist(dto.IdWarehouse))
            return NotFound($"Warehouse with given ID - {dto.IdWarehouse} doesn't exist");
        if (!_service.DoesAmountPositive(dto.Amount))
            return BadRequest("Amount should be a positive value");

        if (!await _repository.DoesOrderExist(dto.IdProduct, dto.Amount, dto.CreatedAt))
            return NotFound($"Order with provided ID product - {dto.IdProduct} and amount - {dto.Amount} and after this date - {dto.CreatedAt} doesn't exist");

        if (!await _repository.DoesOrderCompleted(dto.IdProduct, dto.Amount, dto.CreatedAt))
            return NotFound($"Order with provided ID product - {dto.IdProduct} and amount - {dto.Amount} and after this date - {dto.CreatedAt} has been already completed");

        await _repository.UpdateOrder(dto.IdProduct, dto.Amount, dto.CreatedAt);

        var id = _repository.InsertToProductWarehouse(dto);

        return Ok(id);
    }

    [HttpPost]
    [Route("task2")]
    public async Task<IActionResult> Task2(WarehouseDTO dto)
    {
        var id = await _repository.ExecuteProcedure(dto);
        if (id == 0)
            return BadRequest();
        return Ok(id);
    }
}