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
    public async Task<IActionResult> GetComanda([FromQuery] Guid tableId, [FromQuery] string customer) =>
        Ok(await orderService.GetComandaAsync(tableId, customer));

    // Public: everyone at a table sees all comandas/orders of that table.
    [HttpGet("table/{tableId:guid}")]
    public async Task<IActionResult> GetTableOrders(Guid tableId) =>
        Ok(await orderService.GetTableOrdersAsync(tableId));

    // Public: close (settle) a comanda so it leaves the active tabs and the table
    // can take new customers. Reusing the same name later opens a fresh comanda.
    [HttpPost("comanda/close")]
    public async Task<IActionResult> CloseComanda([FromBody] CloseComandaDto dto)
    {
        await orderService.CloseComandaAsync(dto);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var order = await orderService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDto dto) =>
        Ok(await orderService.UpdateAsync(id, dto));

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await orderService.CancelAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus status)
    {
        await orderService.UpdateStatusAsync(id, status);
        return NoContent();
    }
}
