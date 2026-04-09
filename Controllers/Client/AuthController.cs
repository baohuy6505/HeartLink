using System.Security.Claims;
using HeartLink.Data;
using HeartLink.Models;
using HeartLink.Models.Auth;
using HeartLink.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeartLink.Controllers.Client;

[ApiController]
[Route("api/client/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public AuthController(ApplicationDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var email = request.Email.Trim().ToLower();
        var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        if (await _context.Accounts.AnyAsync(x => x.Email.ToLower() == email))
            return BadRequest(new { message = "Email đã tồn tại." });

        if (!string.IsNullOrWhiteSpace(phone) &&
            await _context.Accounts.AnyAsync(x => x.PhoneNumber == phone))
            return BadRequest(new { message = "Số điện thoại đã tồn tại." });

        var account = new Account
        {
            Email = email,
            PhoneNumber = phone,
            UserRole = "User",
            Status = true,
            CreatedDate = DateTime.Now
        };

        account.Password = _passwordHasher.HashPassword(account, request.Password);

        var profile = new Profile
        {
            Account = account,
            FullName = "Người dùng mới",
            BirthDate = DateTime.Now.AddYears(-20), // Default age 20
            Gender = "Khác",
            TargetGender = "Tất cả",
            MinAge = 18,
            MaxAge = 99,
            Radius = 50,
            CreatedDate = DateTime.Now
        };

        account.Profile = profile;

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(account);

        return Ok(new
        {
            message = "Đăng ký thành công",
            accountId = account.AccountID,
            token
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var keyword = request.EmailOrPhone.Trim().ToLower();

        var account = await _context.Accounts
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x =>
                x.Email.ToLower() == keyword ||
                (x.PhoneNumber != null && x.PhoneNumber == request.EmailOrPhone.Trim()));

        if (account == null)
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });

        if (!account.Status)
            return Unauthorized(new { message = "Tài khoản đã bị khóa." });

        var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.Password, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu." });

        var token = _jwtService.GenerateToken(account);

        return Ok(new
        {
            message = "Đăng nhập thành công",
            token,
            accountId = account.AccountID,
            email = account.Email,
            role = account.UserRole,
            fullName = account.Profile?.FullName
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var accountId = GetCurrentAccountId();

        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
            return NotFound(new { message = "Không tìm thấy tài khoản." });

        var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.Password, request.OldPassword);
        if (verifyResult == PasswordVerificationResult.Failed)
            return BadRequest(new { message = "Mật khẩu cũ không đúng." });

        account.Password = _passwordHasher.HashPassword(account, request.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new
        {
            message = "Đăng xuất thành công."
        });
    }

    private int GetCurrentAccountId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(claim!);
    }
}