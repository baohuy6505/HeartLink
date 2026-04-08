using System.Security.Claims;
using HeartLink.Data;
using HeartLink.Models;
using HeartLink.Models.InteractionDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Controllers.Client;

[ApiController]
[Authorize]
[Route("api/client/messages")]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MessageController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{matchId:int}")]
    public async Task<IActionResult> GetMessages(int matchId)
    {
        var userId = GetUserId();

        var match = await _context.Matches.FirstOrDefaultAsync(x =>
            x.MatchID == matchId &&
            x.Status == 1 &&
            (x.User1ID == userId || x.User2ID == userId));

        if (match == null)
            return NotFound(new { message = "Match không hợp lệ" });

        var messages = await _context.Messages
            .Where(x => x.MatchID == matchId)
            .OrderBy(x => x.SentTime)
            .ToListAsync();

        return Ok(messages);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();

        var match = await _context.Matches.FirstOrDefaultAsync(x =>
            x.MatchID == request.MatchID &&
            x.Status == 1 &&
            (x.User1ID == userId || x.User2ID == userId));

        if (match == null)
            return BadRequest(new { message = "Không thể gửi tin nhắn" });

        var msg = new Message
        {
            MatchID = request.MatchID,
            SenderID = userId,
            Content = request.Content,
            SentTime = DateTime.Now,
            IsRead = false
        };

        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Gửi thành công" });
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}