using System.Security.Claims;
using HeartLink.Data;
using HeartLink.Models;
using HeartLink.Models.ProfileDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Controllers.Client;

[ApiController]
[Authorize]
[Route("api/client/profile")]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var accountId = GetCurrentAccountId();

        var p = await _context.Profiles
            .Include(x => x.Account)
            .Include(x => x.Interests)
            .FirstOrDefaultAsync(x => x.AccountID == accountId);

        if (p == null)
            return NotFound(new { message = "Không tìm thấy hồ sơ." });

        var profile = new
        {
            p.ProfileID,
            p.AccountID,
            Email = p.Account.Email,
            p.Account.PhoneNumber,
            p.Account.UserRole,
            p.FullName,
            p.BirthDate,
            Age = CalculateAge(p.BirthDate),
            p.Gender,
            p.Bio,
            p.Location,
            p.Avatar,
            p.TargetGender,
            p.MinAge,
            p.MaxAge,
            p.Radius,
            p.Latitude,
            p.Longitude,
            Interests = p.Interests.Select(i => i.InterestName).ToList()
        };

        return Ok(profile);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (request.BirthDate.Date > DateTime.Today.AddYears(-18))
            return BadRequest(new { message = "Người dùng phải từ 18 tuổi trở lên." });

        if (request.MinAge > request.MaxAge)
            return BadRequest(new { message = "MinAge không được lớn hơn MaxAge." });

        var accountId = GetCurrentAccountId();

        var profile = await _context.Profiles
            .Include(x => x.Interests)
            .FirstOrDefaultAsync(x => x.AccountID == accountId);

        if (profile == null)
            return NotFound(new { message = "Không tìm thấy hồ sơ." });

        profile.FullName = request.FullName.Trim();
        profile.BirthDate = request.BirthDate.Date;
        profile.Gender = request.Gender;
        profile.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
        profile.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        profile.Avatar = string.IsNullOrWhiteSpace(request.Avatar) ? null : request.Avatar.Trim();
        profile.TargetGender = string.IsNullOrWhiteSpace(request.TargetGender) ? "Tất cả" : request.TargetGender;
        profile.MinAge = request.MinAge;
        profile.MaxAge = request.MaxAge;
        profile.Radius = request.Radius;
        profile.Latitude = request.Latitude;
        profile.Longitude = request.Longitude;

        _context.ProfileInterests.RemoveRange(profile.Interests);

        var newInterests = request.Interests
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .Select(x => new ProfileInterest
            {
                ProfileID = profile.ProfileID,
                InterestName = x
            })
            .ToList();

        await _context.ProfileInterests.AddRangeAsync(newInterests);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật hồ sơ thành công." });
    }

    [HttpPut("filter")]
    public async Task<IActionResult> SetFilter([FromBody] SetFilterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (request.MinAge > request.MaxAge)
            return BadRequest(new { message = "MinAge không được lớn hơn MaxAge." });

        var accountId = GetCurrentAccountId();

        var profile = await _context.Profiles.FirstOrDefaultAsync(x => x.AccountID == accountId);
        if (profile == null)
            return NotFound(new { message = "Không tìm thấy hồ sơ." });

        profile.TargetGender = request.TargetGender;
        profile.MinAge = request.MinAge;
        profile.MaxAge = request.MaxAge;
        profile.Radius = request.Radius;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật bộ lọc thành công." });
    }

    [HttpPut("location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var accountId = GetCurrentAccountId();

        var profile = await _context.Profiles.FirstOrDefaultAsync(x => x.AccountID == accountId);
        if (profile == null)
            return NotFound(new { message = "Không tìm thấy hồ sơ." });

        profile.Latitude = request.Latitude;
        profile.Longitude = request.Longitude;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Cập nhật vị trí thành công." });
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
}