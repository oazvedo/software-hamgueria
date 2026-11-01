using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftwareHamgueria.Application.DTOs.Dish;
using SoftwareHamgueria.Application.Services.Interfaces;

namespace SoftwareHamgueria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DishController(IDishService dishService) : ControllerBase
{
    [HttpGet]
    /// <param name="active">true = only the dishes the kitchen can make (the public menu, spec 024).</param>
    public async Task<IActionResult> GetAll([FromQuery] bool active = false) => Ok(await dishService.GetAllAsync(active));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dish = await dishService.GetByIdAsync(id);
        return dish is null ? NotFound() : Ok(dish);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDishDto dto)
    {
        var dish = await dishService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = dish.Id }, dish);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDishDto dto) =>
        Ok(await dishService.UpdateAsync(id, dto));

    [HttpPatch("{id:guid}/disable")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await dishService.DisableAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/enable")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Enable(Guid id)
    {
        await dishService.EnableAsync(id);
        return NoContent();
    }
}
