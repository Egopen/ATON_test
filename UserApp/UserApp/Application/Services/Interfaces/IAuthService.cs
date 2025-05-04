namespace UserApp.Application.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<string> SignInAsync(string login, string password);
    }
}
