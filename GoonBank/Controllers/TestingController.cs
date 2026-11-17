using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GoonBank.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class TestingController : ControllerBase
{
    private readonly JewBankDbContext _dbContext;
    private readonly IHostEnvironment _env;

    public TestingController(JewBankDbContext dbContext, IHostEnvironment env)
    {
        _dbContext = dbContext;
        _env = env;
    }

    /// <summary>
    /// Resets all data in the database (Development environment only).
    /// </summary>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetDatabase()
    {
        if (!_env.IsDevelopment())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Reset is only allowed in Development mode." });
        }

        // Remove records across all tables
        _dbContext.BlihPayments.RemoveRange(_dbContext.BlihPayments);
        _dbContext.BlihCodes.RemoveRange(_dbContext.BlihCodes);
        _dbContext.BlihProfiles.RemoveRange(_dbContext.BlihProfiles);
        _dbContext.Transactions.RemoveRange(_dbContext.Transactions);
        _dbContext.AuditLogs.RemoveRange(_dbContext.AuditLogs);
        _dbContext.Accounts.RemoveRange(_dbContext.Accounts);

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Database reset successfully. All accounts, transactions, blih records, and audit logs have been deleted."
        });
    }

    /// <summary>
    /// Deletes a specific account and all of its associated transactions and BLIH data.
    /// </summary>
    [HttpDelete("accounts/{id}")]
    public async Task<IActionResult> DeleteAccount(string id)
    {
        if (!_env.IsDevelopment())
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Deleting accounts via test endpoint is only allowed in Development mode." });
        }

        var account = await _dbContext.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound(new { message = $"Account with ID {id} not found." });
        }

        // 1. BLIH Profiles & Codes
        var profileIds = await _dbContext.BlihProfiles
            .Where(b => b.AccountId == id)
            .Select(b => b.BlihProfileId)
            .ToListAsync();

        if (profileIds.Count > 0)
        {
            var blihCodes = await _dbContext.BlihCodes
                .Where(c => profileIds.Contains(c.BlihProfileId))
                .ToListAsync();
            _dbContext.BlihCodes.RemoveRange(blihCodes);
        }

        var blihProfiles = await _dbContext.BlihProfiles
            .Where(b => b.AccountId == id)
            .ToListAsync();
        _dbContext.BlihProfiles.RemoveRange(blihProfiles);

        // 2. BLIH Payments
        var blihPayments = await _dbContext.BlihPayments
            .Where(p => p.PayerAccountId == id)
            .ToListAsync();
        _dbContext.BlihPayments.RemoveRange(blihPayments);

        // 3. Transactions
        var transactions = await _dbContext.Transactions
            .Where(t => t.SourceAccountId == id || t.TargetAccountId == id)
            .ToListAsync();
        _dbContext.Transactions.RemoveRange(transactions);

        // 4. Audit Logs
        var auditLogs = await _dbContext.AuditLogs
            .Where(a => a.CustomerId == id || a.EntityId == id)
            .ToListAsync();
        _dbContext.AuditLogs.RemoveRange(auditLogs);

        // 5. Account
        _dbContext.Accounts.Remove(account);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = $"Account {id} and all related records have been deleted." });
    }
}
