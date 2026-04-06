using HeartLink.Data;
using HeartLink.Models.ProfileDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Controllers.Client
{
    [ApiController]
    [Route("api/client/profile")]
    [Authorize(Roles = "User,Admin")]
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
            var accountIdClaim = User.FindFirst("AccountID")?.Value;
            if (string.IsNullOrWhiteSpace(accountIdClaim))
                return Unauthorized(new { message = "Token không hợp lệ" });

            var accountId = int.Parse(accountIdClaim);

            var profile = await _context.Profiles
                .Include(x => x.Account)
                .Where(x => x.AccountID == accountId)
                .Select(x => new
                {
                    x.ProfileID,
                    x.AccountID,
                    x.FullName,
                    x.BirthDate,
                    x.Gender,
                    x.Bio,
                    x.Location,
                    x.Avatar,
                    x.Interests,
                    x.TargetGender,
                    x.MinAge,
                    x.MaxAge,
                    x.Radius,
                    x.CreatedAt,
                    x.UpdatedAt,
                    Email = x.Account!.Email,
                    PhoneNumber = x.Account!.PhoneNumber,
                    Role = x.Account!.Role,
                    Status = x.Account!.Status
                })
                .FirstOrDefaultAsync();

            if (profile == null)
                return NotFound(new { message = "Không tìm thấy hồ sơ" });

            return Ok(profile);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.BirthDate.HasValue && request.BirthDate.Value.Date > DateTime.Today)
                return BadRequest(new { message = "Ngày sinh không hợp lệ" });

            if (request.MinAge.HasValue && request.MaxAge.HasValue && request.MinAge > request.MaxAge)
                return BadRequest(new { message = "MinAge không được lớn hơn MaxAge" });

            var accountIdClaim = User.FindFirst("AccountID")?.Value;
            if (string.IsNullOrWhiteSpace(accountIdClaim))
                return Unauthorized(new { message = "Token không hợp lệ" });

            var accountId = int.Parse(accountIdClaim);

            var profile = await _context.Profiles.FirstOrDefaultAsync(x => x.AccountID == accountId);
            if (profile == null)
                return NotFound(new { message = "Không tìm thấy hồ sơ" });

            profile.FullName = request.FullName.Trim();
            profile.BirthDate = request.BirthDate;
            profile.Gender = request.Gender;
            profile.Bio = request.Bio;
            profile.Location = request.Location;
            profile.Avatar = request.Avatar;
            profile.Interests = request.Interests;
            profile.TargetGender = request.TargetGender;
            profile.MinAge = request.MinAge;
            profile.MaxAge = request.MaxAge;
            profile.Radius = request.Radius;
            profile.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật hồ sơ thành công" });
        }
    }
}