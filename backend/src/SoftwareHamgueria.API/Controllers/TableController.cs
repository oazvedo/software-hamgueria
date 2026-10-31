using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftwareHamgueria.Application.DTOs.Table;
using SoftwareHamgueria.Application.Services.Interfaces;

namespace SoftwareHamgueria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TableController(ITableService tableService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await tableService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var table = await tableService.GetByIdAsync(id);
        return table is null ? NotFound() : Ok(table);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
    {
        var table = await tableService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = table.Id }, table);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTableDto dto) =>
        Ok(await tableService.UpdateAsync(id, dto));

    [HttpPatch("{id:guid}/disable")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Disable(Guid id)
    {
        await tableService.DisableAsync(id);
        return NoContent();
    }
}
