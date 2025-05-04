using UserApp.Infrastructure.DTO;

namespace UserApp.Infrastructure.Services.Interfaces
{
    public interface ITokenService
    {
        public string CreateTokenAsync(Guid id,string login,bool isAdmin);
        public bool ValidateToken(string token);

        public TokenDataDTO GetTokenData(string token);
    }
}
