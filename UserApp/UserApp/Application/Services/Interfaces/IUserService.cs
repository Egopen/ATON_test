using UserApp.Application.DTO;
using UserApp.Infrastructure.DB.Models;

namespace UserApp.Application.Services.Interfaces
{
    public interface IUserService
    {
        public Task ChangeUserNameGenderBirthdayAsync(ChangeUserNGBDTO changes);
        public Task ChangeUserPasswordByUserAsync(string token, string oldPassword, string newPassword);
        public Task ChangeUserLoginAsync(string token, string login);
        public Task<GetUserDataByUserDTO> GetUserByUserDataAsync(string token, string login, string password);

    }
}
