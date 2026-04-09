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
[Route("api/client/discovery")]
public class DiscoveryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DiscoveryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet("random")]
    public async Task<IActionResult> GetRandom([FromQuery] int take = 6)
    {
        take = Math.Clamp(take, 1, 10);
        var randomProfiles = await _context.Profiles
            .OrderBy(r => Guid.NewGuid())
            .Take(take)
            .Select(x => new
            {
                x.ProfileID,
                x.FullName,
                Age = DateTime.Today.Year - x.BirthDate.Year,
                x.Gender,
                x.Location,
                x.Avatar
            })
            .ToListAsync();

        return Ok(randomProfiles);
    }

    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidates([FromQuery] int take = 10)
    {
        var currentUserId = GetCurrentAccountId();
        take = Math.Clamp(take, 1, 20);

        var myProfile = await _context.Profiles.FirstOrDefaultAsync(x => x.AccountID == currentUserId);
        if (myProfile == null)
            return NotFound(new { message = "Không tìm thấy hồ sơ hiện tại." });

        var interactions = await _context.Likes
            .Where(x => x.SenderID == currentUserId)
            .Select(x => new { x.ReceiverID, x.Type })
            .ToListAsync();

        var likedIds = interactions.Where(i => i.Type == "Like").Select(i => i.ReceiverID).ToList();
        var dislikedIds = interactions.Where(i => i.Type == "Dislike").Select(i => i.ReceiverID).ToList();

        var matchedIds = await _context.Matches
            .Where(x => x.Status == 1 && (x.User1ID == currentUserId || x.User2ID == currentUserId))
            .Select(x => x.User1ID == currentUserId ? x.User2ID : x.User1ID)
            .ToListAsync();

        var likedAndMatchedIds = likedIds.Union(matchedIds).ToList();
        var allExcludedIds = likedAndMatchedIds.Union(dislikedIds).ToList();

        var today = DateTime.Today;
        var minBirthDate = today.AddYears(-(myProfile.MaxAge + 1)).AddDays(1);
        var maxBirthDate = today.AddYears(-myProfile.MinAge);

        var query = _context.Profiles
            .Include(x => x.Interests)
            .Where(x => x.AccountID != currentUserId);

        if (!string.IsNullOrWhiteSpace(myProfile.TargetGender) && myProfile.TargetGender != "Tất cả")
        {
            query = query.Where(x => x.Gender == myProfile.TargetGender);
        }

        query = query.Where(x => x.BirthDate >= minBirthDate && x.BirthDate <= maxBirthDate);

        // Filter out banned/inactive accounts
        query = query.Where(x => x.Account.Status == true);

        // Fetch all potential candidates to filter by distance in memory 
        // (A more optimized approach would be a bounding box filter first)
        var allCandidates = await query
            .Where(x => !allExcludedIds.Contains(x.AccountID))
            .ToListAsync();

        if (myProfile.Latitude.HasValue && myProfile.Longitude.HasValue)
        {
            allCandidates = allCandidates
                .Where(x => !x.Latitude.HasValue || !x.Longitude.HasValue || 
                            CalculateDistance(myProfile.Latitude.Value, myProfile.Longitude.Value, 
                                              x.Latitude.Value, x.Longitude.Value) <= myProfile.Radius)
                .ToList();
        }

        var candidates = allCandidates
            .OrderByDescending(x => x.CreatedDate)
            .Take(take)
            .Select(x => new
            {
                x.ProfileID,
                x.AccountID,
                x.FullName,
                Age = CalculateAge(x.BirthDate),
                x.Gender,
                x.Bio,
                x.Location,
                x.Avatar,
                Interests = x.Interests.Select(i => i.InterestName).ToList()
            })
            .ToList();

        // Stage 2: Fallback to users who were 'Disliked' (Passed) if no new users found
        if (!candidates.Any() && dislikedIds.Any())
        {
            var recycledQuery = query
                .Where(x => !likedAndMatchedIds.Contains(x.AccountID))
                .Where(x => dislikedIds.Contains(x.AccountID));

            var recycledCandidatesFull = await recycledQuery.ToListAsync();

            if (myProfile.Latitude.HasValue && myProfile.Longitude.HasValue)
            {
                recycledCandidatesFull = recycledCandidatesFull
                    .Where(x => !x.Latitude.HasValue || !x.Longitude.HasValue || 
                                CalculateDistance(myProfile.Latitude.Value, myProfile.Longitude.Value, 
                                                  x.Latitude.Value, x.Longitude.Value) <= myProfile.Radius)
                    .ToList();
            }

            candidates = recycledCandidatesFull
                .OrderBy(x => Guid.NewGuid()) 
                .Take(take)
                .Select(x => new
                {
                    x.ProfileID,
                    x.AccountID,
                    x.FullName,
                    Age = CalculateAge(x.BirthDate),
                    x.Gender,
                    x.Bio,
                    x.Location,
                    x.Avatar,
                    Interests = x.Interests.Select(i => i.InterestName).ToList()
                })
                .ToList();
        }

        return Ok(candidates);
    }

    [HttpPost("swipe")]
    public async Task<IActionResult> Swipe([FromBody] SwipeRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var senderId = GetCurrentAccountId();
        var receiverId = request.ReceiverID;

        if (senderId == receiverId)
            return BadRequest(new { message = "Không thể tự tương tác với chính mình." });

        var receiverExists = await _context.Accounts.AnyAsync(x => x.AccountID == receiverId && x.Status);
        if (!receiverExists)
            return NotFound(new { message = "Người nhận không tồn tại hoặc đã bị khóa." });

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            Like? currentLike = await _context.Likes
                .FirstOrDefaultAsync(x => x.SenderID == senderId && x.ReceiverID == receiverId);

            if (currentLike == null)
            {
                currentLike = new Like
                {
                    SenderID = senderId,
                    ReceiverID = receiverId,
                    Type = request.Type,
                    Timestamp = DateTime.Now
                };

                _context.Likes.Add(currentLike);
                await _context.SaveChangesAsync();
            }
            else
            {
                currentLike.Type = request.Type;
                currentLike.Timestamp = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            var isMatched = false;
            int? newMatchId = null;

            if (request.Type == "Like")
            {
                var reverseLikeExists = await _context.Likes.AnyAsync(x =>
                    x.SenderID == receiverId &&
                    x.ReceiverID == senderId &&
                    x.Type == "Like");

                var matchExists = await _context.Matches.AnyAsync(x =>
                    (x.User1ID == senderId && x.User2ID == receiverId) ||
                    (x.User1ID == receiverId && x.User2ID == senderId));

                if (reverseLikeExists && !matchExists)
                {
                    var user1 = Math.Min(senderId, receiverId);
                    var user2 = Math.Max(senderId, receiverId);

                    var match = new Match
                    {
                        LikeID = currentLike.LikeID,
                        User1ID = user1,
                        User2ID = user2,
                        MatchedDate = DateTime.Now,
                        Status = 1
                    };

                    _context.Matches.Add(match);
                    await _context.SaveChangesAsync();

                    isMatched = true;
                    newMatchId = match.MatchID;
                }
            }

            await transaction.CommitAsync();

            return Ok(new
            {
                message = isMatched ? "Kết đôi thành công." : "Đã ghi nhận tương tác.",
                isMatched,
                matchId = newMatchId
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private int GetCurrentAccountId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(claim!);
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var d1 = lat1 * (Math.PI / 180.0);
        var num1 = lon1 * (Math.PI / 180.0);
        var d2 = lat2 * (Math.PI / 180.0);
        var num2 = (lon2 - lon1) * (Math.PI / 180.0);
        var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                 Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
        return 6371.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
    }
}