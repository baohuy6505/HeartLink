using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HeartLink.Models;
using Microsoft.IdentityModel.Tokens;

namespace HeartLink.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Account account)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"];

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, account.AccountID.ToString()),
            new Claim(ClaimTypes.Email, account.Email),

            // 🔥 SỬA Ở ĐÂY
            new Claim(ClaimTypes.Role, account.UserRole),

            new Claim(ClaimTypes.Name, account.Email)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}