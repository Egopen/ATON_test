using System.Security.Cryptography;
using System.Text;
using UserApp.Infrastructure.Services.Interfaces;

namespace UserApp.Infrastructure.Services.implementations
{
    public class HashService : IHashService
    {
        public string HashString(string input)
        {
            var byteInp = Encoding.ASCII.GetBytes(input);
            var hashInp = MD5.HashData(byteInp);
            int i;
            StringBuilder sOutput = new StringBuilder(hashInp.Length);
            for (i = 0; i < hashInp.Length; i++)
            {
                sOutput.Append(hashInp[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    }
}
