using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace UserApp.Infrastructure.Settings
{
    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer";
        public const string AUDIENCE = "MyAuthClient";
        static string KEY = Environment.GetEnvironmentVariable("SECRETKEY");
        public static SymmetricSecurityKey GetSymSecurityKey()
        {
            if (string.IsNullOrEmpty(KEY))
            {
                throw new ArgumentNullException(nameof(KEY), "Ключ безопасности не был загружен.");
            }
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }
    }
}
