using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UserApp.Infrastructure.DB.Models;
using UserApp.Infrastructure.DTO;
using UserApp.Infrastructure.Services.Interfaces;
using UserApp.Infrastructure.Settings;

namespace UserApp.Infrastructure.Services.implementations
{
    public class TokenService:ITokenService
    {
        public string CreateTokenAsync(Guid id, string login, bool isAdmin)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, id.ToString()),
                new(ClaimTypes.Role, isAdmin? "Admin" : "User"),
                new(ClaimTypes.Name, login)
            };

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymSecurityKey(), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = AuthOptions.GetSymSecurityKey();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.ISSUER,
                    ValidateAudience = true,
                    ValidAudience = AuthOptions.AUDIENCE,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero 
                }, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public TokenDataDTO GetTokenData(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                if (jwtToken == null)
                {
                    throw new Exception("Невалидный токен");
                }
                var guidClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (guidClaim == null)
                {
                    throw new Exception("Неправильный токен");
                }
                var login = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                if (login == null) {
                    throw new Exception("Неправильный токен");
                }
                var tokenData = new TokenDataDTO
                {
                    Guid = Guid.Parse(guidClaim),
                    Login = login, 
                    isAdmin = jwtToken.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin")
                };

                return tokenData;
            }
            catch (Exception ex)
            {
                throw new Exception("Не удалось извлечь данные из токена", ex);
            }
        }
    }
}
