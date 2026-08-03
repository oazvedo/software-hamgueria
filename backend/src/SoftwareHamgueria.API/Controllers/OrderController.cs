using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftwareHamgueria.Application.DTOs.Order;
using SoftwareHamgueria.Application.Services.Interfaces;
using SoftwareHamgueria.Domain.Entities;

namespace SoftwareHamgueria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() => Ok(await orderService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await orderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    // Public: a customer follows their own tab (all orders for table + name).
    [HttpGet("comanda")]
    public async Task<IActionResult> GetComanda([FromQuery] Guid tableId, [FromQuery] string customer)
    {
        if (tableId == Guid.Empty || string.IsNullOrWhiteSpace(customer))
            return BadRequest("tableId and customer are required.");
        return Ok(await orderService.GetComandaAsync(tableId, customer));
    }

    // Public: everyone at a table sees all comandas/orders of that table.
    [HttpGet("table/{tableId:guid}")]
    public async Task<IActionResult> GetTableOrders(Guid tableId) =>
        Ok(await orderService.GetTableOrdersAsync(tableId));

    // Public: close (settle) a comanda so it leaves the active tabs and the table
    // can take new customers. Reusing the same name later opens a fresh comanda.
    [HttpPost("comanda/close")]
    public async Task<IActionResult> CloseComanda([FromBody] CloseComandaDto dto)
    {
        if (dto.TableId == Guid.Empty || string.IsNullOrWhiteSpace(dto.CustomerName))
            return BadRequest("tableId and customerName are required.");
        await orderService.CloseComandaAsync(dto.TableId, dto.CustomerName);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            var order = await orderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDto dto)
    {
        try { return Ok(await orderService.UpdateAsync(id, dto)); }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try { await orderService.CancelAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus status)
    {
        try { await orderService.UpdateStatusAsync(id, status); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
