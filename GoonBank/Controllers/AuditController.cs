using GoonBank.Enums;
using GoonBank.Extensions;
using GoonBank.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoonBank.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly JewBankDbContext _dbContext;
    private readonly IHostEnvironment _env;

    public AuditController(JewBankDbContext dbContext, IHostEnvironment env)
    {
        _dbContext = dbContext;
        _env = env;
    }

    /// <summary>
    /// Gets audit logs for the authenticated user.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetMyAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] AuditEntityType? entityType = null)
    {
        var accountId = User.GetAccountId();
        if (string.IsNullOrEmpty(accountId))
        {
            return Unauthorized();
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 200) pageSize = 200;

        var query = _dbContext.AuditLogs
            .Where(a => a.CustomerId == accountId || a.EntityId == accountId || a.ActorIdentityId == accountId);

        if (entityType.HasValue)
        {
            query = query.Where(a => a.EntityType == entityType.Value);
        }

        var logs = await query
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(logs);
    }

    /// <summary>
    /// Gets all audit logs in the system (Development environment only).
    /// </summary>
    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetAllAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] AuditEntityType? entityType = null)
    {
        if (!_env.IsDevelopment())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Viewing all audit logs is restricted to Development mode." });
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 200) pageSize = 200;

        var query = _dbContext.AuditLogs.AsQueryable();

        if (entityType.HasValue)
        {
            query = query.Where(a => a.EntityType == entityType.Value);
        }

        var logs = await query
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(logs);
    }
}
