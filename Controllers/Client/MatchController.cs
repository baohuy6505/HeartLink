using System.Security.Claims;
using HeartLink.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Controllers.Client;

[ApiController]
[Authorize]
[Route("api/client/matches")]
public class MatchController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MatchController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyMatches()
    {
        var currentUserId = GetCurrentAccountId();

        var matches = await _context.Matches
            .Where(x => x.Status == 1 && (x.User1ID == currentUserId || x.User2ID == currentUserId))
            .OrderByDescending(x => x.MatchedDate)
            .Select(x => new
            {
                x.MatchID,
                x.MatchedDate,
                OtherUserId = x.User1ID == currentUserId ? x.User2ID : x.User1ID,
                OtherUserName = x.User1ID == currentUserId ? x.User2.Profile!.FullName : x.User1.Profile!.FullName,
                OtherUserAvatar = x.User1ID == currentUserId ? x.User2.Profile!.Avatar : x.User1.Profile!.Avatar,
                LastMessageTime = x.Messages.OrderByDescending(m => m.SentTime).Select(m => (DateTime?)m.SentTime).FirstOrDefault(),
                LastMessage = x.Messages.OrderByDescending(m => m.SentTime).Select(m => m.Content).FirstOrDefault(),
                TotalMessages = x.Messages.Count,
                UnreadCount = x.Messages.Count(m => !m.IsRead && m.SenderID != currentUserId)
            })
            .ToListAsync();

        return Ok(matches);
    }

    [HttpPost("{matchId:int}/cancel")]
    public async Task<IActionResult> CancelMatch(int matchId)
    {
        var currentUserId = GetCurrentAccountId();

        var match = await _context.Matches.FirstOrDefaultAsync(x =>
            x.MatchID == matchId &&
            x.Status == 1 &&
            (x.User1ID == currentUserId || x.User2ID == currentUserId));

        if (match == null)
            return NotFound(new { message = "Không tìm thấy match hợp lệ." });

        match.Status = 2;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã hủy kết đôi." });
    }

    private int GetCurrentAccountId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(claim!);
    }
}