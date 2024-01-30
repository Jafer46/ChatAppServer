using System.Security.Claims;
using ChatAppServer.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ChatAppServer.Interfaces;

namespace ChatAppServer.Services
{
    public class AuthenticationService : IAuthentication
    {
        public async Task<dynamic> GenerateJwtToken(User user, string jwtsecret)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentNullException(nameof(user.Username));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds().ToString()),
            };

            var token = new JwtSecurityToken(
                new JwtHeader(
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtsecret)), SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));

            dynamic output = new
            {
                Access_Token = new JwtSecurityTokenHandler().WriteToken(token),
                user.Username,
            };

            return await Task.FromResult(output);
        }
    }
}