using UserApp.Infrastructure.DB.Models;

namespace UserApp.Infrastructure.Services.Interfaces
{
    public interface IUserRepository
    {
        public Task<User?> GetUserByIdAsync(Guid id);
        public Task<User?> GetUserByLoginAsync(string login);
        public Task AddUserAsync(User user);
        public Task DeleteUserAsync(User user, string revokedBy);
        public Task<List<User>> GetAllActiveUsersAsync();
        public Task<List<User>> GetAllOlderASync(int age);
        public Task SaveChangesAsync();
    }
}
