using Microsoft.IdentityModel.Tokens;
using StudentGroup.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

namespace StudentGroup.Services
{
    public class JwtService
    {
        public string GenerateToken(AppUser user, IList<string>roles, IConfiguration config)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("Fullname", user.FullName)
        };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));



            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwtSecurityToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
               issuer: config["Jwt:Issuer"],
               audience: config["Jwt:Audience"],
                claims: claims,
               expires: DateTime.Now.AddMinutes(15),
               signingCredentials: creds
           );
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return token;
        }

    }
}
