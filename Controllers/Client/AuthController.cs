using HeartLink.Data;
using HeartLink.Models;
using HeartLink.Models.Auth;
using HeartLink.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Controllers.Client
{
    [ApiController]
    [Route("api/client/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Account> _hasher;
        private readonly JwtService _jwtService;

        public AuthController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _hasher = new PasswordHasher<Account>();
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest(new { message = "Phải nhập Email hoặc Số điện thoại" });

            if (request.BirthDate.HasValue && request.BirthDate.Value.Date > DateTime.Today)
                return BadRequest(new { message = "Ngày sinh không hợp lệ" });

            if (request.BirthDate.HasValue)
            {
                var age = DateTime.Today.Year - request.BirthDate.Value.Year;
                if (request.BirthDate.Value.Date > DateTime.Today.AddYears(-age)) age--;
                if (age < 18)
                    return BadRequest(new { message = "Người dùng phải từ 18 tuổi trở lên" });
            }

            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLower();
            var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            var existsByEmail = email != null &&
                                await _context.Accounts.AnyAsync(x => x.Email != null && x.Email.ToLower() == email);

            if (existsByEmail)
                return BadRequest(new { message = "Email đã được sử dụng" });

            var existsByPhone = phone != null &&
                                await _context.Accounts.AnyAsync(x => x.PhoneNumber == phone);

            if (existsByPhone)
                return BadRequest(new { message = "Số điện thoại đã được sử dụng" });

            var account = new Account
            {
                Email = email,
                PhoneNumber = phone,
                Status = "Active",
                Role = "User",
                CreatedDate = DateTime.Now
            };

            account.PasswordHash = _hasher.HashPassword(account, request.Password);

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var profile = new Profile
            {
                AccountID = account.AccountID,
                FullName = request.FullName.Trim(),
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                CreatedAt = DateTime.Now
            };

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đăng ký thành công",
                accountId = account.AccountID,
                role = account.Role
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest(new { message = "Phải nhập Email hoặc Số điện thoại" });

            var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLower();
            var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(x =>
                    (email != null && x.Email != null && x.Email.ToLower() == email) ||
                    (phone != null && x.PhoneNumber == phone));

            if (account == null)
                return BadRequest(new { message = "Tài khoản không tồn tại" });

            if (account.Role != "User" && account.Role != "Admin")
                return BadRequest(new { message = "Vai trò tài khoản không hợp lệ" });

            if (account.Status == "Banned")
                return BadRequest(new { message = "Tài khoản đã bị khóa" });

            if (account.Status != "Active")
                return BadRequest(new { message = "Tài khoản không hoạt động" });

            var result = _hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                return BadRequest(new { message = "Mật khẩu không đúng" });

            account.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(account);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token,
                accountId = account.AccountID,
                role = account.Role
            });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.NewPassword != request.ConfirmNewPassword)
                return BadRequest(new { message = "Xác nhận mật khẩu mới không khớp" });

            if (request.OldPassword == request.NewPassword)
                return BadRequest(new { message = "Mật khẩu mới không được trùng mật khẩu cũ" });

            var accountIdClaim = User.FindFirst("AccountID")?.Value;
            if (string.IsNullOrWhiteSpace(accountIdClaim))
                return Unauthorized(new { message = "Token không hợp lệ" });

            var accountId = int.Parse(accountIdClaim);

            var account = await _context.Accounts.FirstOrDefaultAsync(x => x.AccountID == accountId);
            if (account == null)
                return NotFound(new { message = "Không tìm thấy tài khoản" });

            var verifyOldPassword = _hasher.VerifyHashedPassword(account, account.PasswordHash, request.OldPassword);
            if (verifyOldPassword == PasswordVerificationResult.Failed)
                return BadRequest(new { message = "Mật khẩu cũ không đúng" });

            account.PasswordHash = _hasher.HashPassword(account, request.NewPassword);
            account.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công" });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                message = "Đăng xuất thành công."
            });
        }
    }
}