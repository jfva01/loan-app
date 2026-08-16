using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ExternalService.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class LoanRecordsController : ControllerBase
{
    private readonly InMemoryStore _store;
    private readonly ILogger<LoanRecordsController> _logger;

    public LoanRecordsController(InMemoryStore store, ILogger<LoanRecordsController> logger)
    {
        _store = store;
        _logger = logger;
    }

    // Nuevo cliente -> (create)
    [HttpPost]
    public IActionResult Create([FromBody] JsonElement payload)
    {
        var customerId = payload.GetProperty("customerId").GetGuid();
        _store.Records[customerId] = payload;
        _logger.LogInformation("CREATE received for {CustomerId}: {Payload}", customerId, payload);
        return Ok(new { received = true, operation = "create", customerId, payload });
    }

    // Cliente recurrente -> (update)
    [HttpPut("{customerId:guid}")]
    public IActionResult Update(Guid customerId, [FromBody] JsonElement payload)
    {
        _store.Records[customerId] = payload;
        _logger.LogInformation("UPDATE received for {CustomerId}: {Payload}", customerId, payload);
        return Ok(new { received = true, operation = "update", customerId, payload });
    }

    // Verificar recepción de los datos
    [HttpGet]
    public IActionResult GetAll() => Ok(_store.Records);
}