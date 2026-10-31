using Microsoft.AspNetCore.Mvc;

namespace SoftwareHamgueria.FunctionalTests.Infrastructure;

/// <summary>Test-only route that fails unexpectedly, to prove internal details never reach the client (003/CA-10).</summary>
[ApiController]
[Route("__test/boom")]
public class BoomController : ControllerBase
{
    public const string Secret = "SEGREDO-interno-da-consulta";

    [HttpGet]
    public IActionResult Get() => throw new InvalidOperationException(Secret);
}
