using HeartLink.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _context.Accounts
            .Include(x => x.Profile)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();

            query = query.Where(x =>
                x.Email.Contains(keyword) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(keyword)) ||
                (x.Profile != null && x.Profile.FullName.Contains(keyword)));
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.AccountID,
                x.Email,
                x.PhoneNumber,
                x.CreatedDate,
                x.Status,
                x.UserRole,
                FullName = x.Profile != null ? x.Profile.FullName : null,
                Gender = x.Profile != null ? x.Profile.Gender : null,
                Location = x.Profile != null ? x.Profile.Location : null
            })
            .ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalItems,
            items
        });
    }

    [HttpPost("users/{accountId:int}/ban")]
    public async Task<IActionResult> BanUser(int accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
            return NotFound(new { message = "Không tìm thấy tài khoản." });

        if (account.UserRole == "Admin")
            return BadRequest(new { message = "Không thể khóa tài khoản Admin." });

        account.Status = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Khóa tài khoản thành công." });
    }

    [HttpPost("users/{accountId:int}/unban")]
    public async Task<IActionResult> UnbanUser(int accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
            return NotFound(new { message = "Không tìm thấy tài khoản." });

        account.Status = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Mở khóa tài khoản thành công." });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStatistics()
    {
        var fromDate = DateTime.Today.AddDays(-30);

        var totalUsers = await _context.Accounts.CountAsync();
        var activeUsers = await _context.Accounts.CountAsync(x => x.Status);
        var bannedUsers = await _context.Accounts.CountAsync(x => !x.Status);
        var totalMatches = await _context.Matches.CountAsync();
        var activeMatches = await _context.Matches.CountAsync(x => x.Status == 1);
        var cancelledMatches = await _context.Matches.CountAsync(x => x.Status == 2);
        var totalMessages = await _context.Messages.CountAsync();

        var newUsersByDate = await _context.Accounts
            .Where(x => x.CreatedDate >= fromDate)
            .GroupBy(x => x.CreatedDate.Date)
            .Select(g => new
            {
                RegisterDate = g.Key,
                TotalNewUsers = g.Count()
            })
            .OrderBy(x => x.RegisterDate)
            .ToListAsync();

        var popularProfiles = await _context.Likes
            .Where(x => x.Type == "Like")
            .GroupBy(x => x.ReceiverID)
            .Select(g => new
            {
                AccountID = g.Key,
                TotalLikes = g.Count()
            })
            .OrderByDescending(x => x.TotalLikes)
            .Take(5)
            .Join(_context.Profiles,
                like => like.AccountID,
                profile => profile.AccountID,
                (like, profile) => new
                {
                    like.AccountID,
                    profile.FullName,
                    like.TotalLikes
                })
            .ToListAsync();

        return Ok(new
        {
            totalUsers,
            activeUsers,
            bannedUsers,
            totalMatches,
            activeMatches,
            cancelledMatches,
            totalMessages,
            newUsersByDate,
            popularProfiles
        });
    }
}