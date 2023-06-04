using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TeamCubing.BLL.Interfaces;
using TeamCubing.BLL.Settings;
using TeamCubing.DAL.Models;

namespace TeamCubing.BLL.Services;

public class JwtGenerator : IJwtGenerator
{
    private readonly SymmetricSecurityKey _key;
    private readonly JwtSettings _settings;

    public JwtGenerator(IOptions<JwtSettings> config)
    {
        _settings = config.Value;
        _key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.TokenKey));
    }

    public string GenerateToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.NameId, user.UserName)
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddHours(_settings.ExpiresInHours),
            SigningCredentials = credentials,
        };
        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
