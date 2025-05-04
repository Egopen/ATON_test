using UserApp.Application.DTO;
using UserApp.Infrastructure.DB.Models;

namespace UserApp.Application.Services.Interfaces
{
    public interface IAdminService
    {
        public Task<GetUserInfoByAdminDTO> GetUserByIdAsync(string token, Guid id);
        public Task<GetUserInfoByAdminDTO> GetUserByLoginAsync(string token, string login);
        public Task CreateUserAsync(CreateUserDTO user);
        public Task ChangeUserPasswordByAdminAsync(string token, string login, string newPassword);
        public Task ChangeUserNameGenderBirthdayAsync(ChangeUserByAdminNGBDTO changes);
        public Task ChangeUserLoginAsync(string token,string oldlogin, string login);
        public Task<List<User>> GetAllActiveUsersAsync(string token);
        public Task<List<User>> GetAllUsersOlderThanDataAsync(string token, int years);
        public Task RemoveUserSoftlyByLoginAsync(string token, string login);
        public Task RestoreUserByLoginAsync(string token, string login);
    }
}
