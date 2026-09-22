using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Utilities.Helpers;

namespace WebApi.Features.Health;

[Route("api/health")]
[ApiController]
[AllowAnonymous]
public class HealthController(DbHelper dbHelper) : ControllerBase
{
    /// <summary>
    /// Liveness/readiness probe. No auth required — use to verify the API is reachable on the server.
    /// GET /api/health
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        string databaseStatus;
        string? databaseDetail = null;

        try
        {
            await using var conn = dbHelper.GetConnection();
            await conn.OpenAsync(ct);
            var one = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition("SELECT 1", commandType: CommandType.Text, cancellationToken: ct));
            databaseStatus = one == 1 ? "Healthy" : "Unhealthy";
        }
        catch (Exception ex)
        {
            databaseStatus = "Unhealthy";
            databaseDetail = ex.Message;
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                api = "Healthy",
                database = databaseStatus,
                detail = databaseDetail,
                utc = DateTime.UtcNow
            });
        }

        return Ok(new
        {
            status = "Healthy",
            api = "Healthy",
            database = databaseStatus,
            utc = DateTime.UtcNow
        });
    }
}
