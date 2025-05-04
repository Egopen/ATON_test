using Microsoft.EntityFrameworkCore;
using UserApp.Infrastructure.DB;
using UserApp.Infrastructure.DB.Models;
using UserApp.Infrastructure.Services.Interfaces;

namespace UserApp.Infrastructure.Services.implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly DBContext _db;
        public UserRepository(DBContext db)
        {
            _db = db;
        }
        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Guid == id);
            return user;
        }
        public async Task<User?> GetUserByLoginAsync(string login)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Login == login);
            return user;
        }
        public async Task AddUserAsync(User user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }
        public async Task DeleteUserAsync(User user, string revokedBy)
        {
            user.RevokedOn=DateTime.UtcNow;
            user.RevokedBy=revokedBy;
            await _db.SaveChangesAsync();

        }
        public async Task<List<User>> GetAllActiveUsersAsync()
        {
            var users = await _db.Users
            .Where(u => u.RevokedOn == null)
            .OrderBy(u => u.CreatedOn)
            .ToListAsync();
            return users;
        }
        public async Task<List<User>> GetAllOlderASync(int age)
        {
            
            var users = await _db.Users.Where(u => u.Birthday!=null && DateTime.UtcNow.Year-u.Birthday.Value.Year > age).ToListAsync();
            return users;
        }
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
