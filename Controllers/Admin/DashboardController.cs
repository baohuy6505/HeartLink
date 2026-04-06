using HeartLink.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            var accounts = await _context.Accounts
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new
                {
                    x.AccountID,
                    x.Email,
                    x.PhoneNumber,
                    x.Role,
                    x.Status,
                    x.CreatedDate,
                    x.LastLoginAt
                })
                .ToListAsync();

            return Ok(accounts);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var totalAccounts = await _context.Accounts.CountAsync();
            var totalUsers = await _context.Accounts.CountAsync(x => x.Role == "User");
            var totalAdmins = await _context.Accounts.CountAsync(x => x.Role == "Admin");
            var activeAccounts = await _context.Accounts.CountAsync(x => x.Status == "Active");
            var bannedAccounts = await _context.Accounts.CountAsync(x => x.Status == "Banned");

            return Ok(new
            {
                totalAccounts,
                totalUsers,
                totalAdmins,
                activeAccounts,
                bannedAccounts
            });
        }
    }
}