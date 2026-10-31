using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftwareHamgueria.Application.DTOs.Audit;
using SoftwareHamgueria.Application.Services.Interfaces;

namespace SoftwareHamgueria.API.Controllers;

/// <summary>Who changed what, when and from which screen (ADR 0027). Kitchen only.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditController(IAuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AuditQuery query) => Ok(await auditService.GetAsync(query));
}
