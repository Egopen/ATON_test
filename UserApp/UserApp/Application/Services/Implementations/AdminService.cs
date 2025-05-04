using System.Text.RegularExpressions;
using UserApp.Application.DTO;
using UserApp.Application.Errors;
using UserApp.Application.Services.Interfaces;
using UserApp.Infrastructure.DB.Models;
using UserApp.Infrastructure.DTO;
using UserApp.Infrastructure.Services.Interfaces;

namespace UserApp.Application.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<AdminService> _logger;
        private readonly IHashService _hashService;
        private readonly ITokenService _tokenService;

        public AdminService(IUserRepository repository, ILogger<AdminService> logger, IHashService hashService, ITokenService tokenService)
        {
            _repository = repository;
            _logger = logger;
            _hashService = hashService;
            _tokenService = tokenService;
        }
        private TokenDataDTO ValidateAndGetTokenData(string token)
        {
            _logger.LogInformation("Валидация токена");
            if (!_tokenService.ValidateToken(token))
            {
                throw new InvalidTokenException("Токен недействителен или данные неправильные");
            }

            _logger.LogInformation("Токен валиден. Получение данных из токена");
            var tokendata = _tokenService.GetTokenData(token);
            if (!tokendata.isAdmin)
            {
                _logger.LogWarning($"Пользователь с ID: {tokendata.Guid} не имеет прав администратора");
                throw new UserNotAdminException("У пользователя нет прав администратора");
            }
            return tokendata;
        }
        public async Task<GetUserInfoByAdminDTO> GetUserByIdAsync(string token, Guid id)
        {

            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка получить пользователя с ID: {id}");

                var user = await _repository.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с ID: {id} не найден");
                    throw new UserNotExistsException($"Пользователь с ID: {id} не найден");
                }
                var userDTO = new GetUserInfoByAdminDTO
                {
                    Name = user.Name,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    IsActive = user.RevokedOn is null
                };
                _logger.LogInformation($"Пользователь с ID: {id} найден!");
                return userDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке найти пользователя с ID: {id}");
                throw;
            }
        }

        public async Task<GetUserInfoByAdminDTO> GetUserByLoginAsync(string token, string login)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка получить пользователя с логином: {login}");
                var user = await _repository.GetUserByLoginAsync(login);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} не найден");
                    throw new UserNotExistsException($"Пользователь с логином: {login} не найден");
                }

                var userDTO = new GetUserInfoByAdminDTO
                {
                    Name = user.Name,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    IsActive = user.RevokedOn is null
                };
                _logger.LogInformation($"Пользователь с логином: {login} найден!");
                return userDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке найти пользователя с логином: {login}");
                throw;
            }
        }
        public async Task CreateUserAsync(CreateUserDTO user)
        {
            _logger.LogInformation($"Попытка добавить пользователя с логином: {user.Login}");
            try
            {
                var tokendata = ValidateAndGetTokenData(user.Token);
                if (!Regex.IsMatch(user.Password, "^[a-zA-Z0-9]+$") || user.Password.Length < 8)
                {
                    _logger.LogWarning("Введен неверный формат пароля");
                    throw new WrongDataException("Введен неверный формат пароля, только латинские буквы и цифры, больше 7 символов");
                }
                if (!Regex.IsMatch(user.Login, "^[a-zA-Z0-9]+$"))
                {
                    throw new WrongDataException("Неправильный формат логина, только латиские буквы и цифры");
                }
                if (!Regex.IsMatch(user.Name, "^^[a-zA-Zа-яА-ЯёЁ\\s]+$"))
                {
                    throw new WrongDataException("Неправильный формат имени, только латиские буквы и русские буквы");
                }
                if (user.Gender >= 3 && user.Gender < 0)
                {
                    throw new WrongDataException("Неправильный формат гендера, должен быть только от 0 до 2");
                }
                await _repository.AddUserAsync(new User
                {
                    Login = user.Login,
                    Password = _hashService.HashString(user.Password),
                    Name = user.Name,
                    Gender = user.Gender,
                    Birthday = user.Birthday,
                    Admin = user.Admin,
                    CreatedBy = tokendata.Login,
                    ModifiedBy = tokendata.Login
                });
                _logger.LogInformation($"Пользователь с логином {user.Login} успешно добавлен.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке добавить пользователя с логином: {user.Login}");
                throw;
            }
        }
        public async Task ChangeUserNameGenderBirthdayAsync(ChangeUserByAdminNGBDTO changes)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(changes.Token);
                _logger.LogInformation($"Попытка поменять имя или др или пол у пользователя с логином: {changes.Login}");
                var user = await _repository.GetUserByLoginAsync(changes.Login);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином: {changes.Login} не найден");
                    throw new UserNotExistsException($"Пользователь с логином: {changes.Login} не найден");
                }
                var isUpdated = false;
                if (!string.IsNullOrEmpty(changes.Name) && user.Name != changes.Name)
                {
                    if (Regex.IsMatch(changes.Name, "^^[a-zA-Zа-яА-ЯёЁ\\s]+$"))
                    {
                        throw new WrongDataException("Неправильный формат имени, только латиские буквы и русские буквы");
                    }
                    user.Name = changes.Name;
                    isUpdated = true;
                }
                if (changes.Gender.HasValue && user.Gender != changes.Gender)
                {
                    if (user.Gender >= 3 && user.Gender < 0)
                    {
                        throw new WrongDataException("Неправильный формат гендера, должен быть только от 0 до 2");
                    }
                    user.Gender = (int)changes.Gender;
                    isUpdated = true;
                }
                if (changes.Birthday.HasValue && user.Birthday != changes.Birthday)
                {
                    user.Birthday = changes.Birthday;
                    isUpdated = true;
                }
                if (isUpdated)
                {
                    user.ModifiedOn = DateTime.UtcNow;
                    user.ModifiedBy = tokendata.Login;
                }
                await _repository.SaveChangesAsync();
                _logger.LogInformation($"Попытка поменять имя или др или пол у пользователя с логином: {changes.Login} прошла успешно");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке поменять имя или др или пол у пользователя с логином: {changes.Login}. Ошибка: {ex.Message}");
                throw;
            }
        }
        public async Task ChangeUserPasswordByAdminAsync(string token, string login, string newPassword)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка поменять пароль у пользователя с логином: {login}");
                if (!Regex.IsMatch(newPassword, "^[a-zA-Z0-9]+$") || newPassword.Length < 8)
                {
                    _logger.LogWarning("Введен неверный формат пароля");
                    throw new WrongDataException("Введен неверный формат пароля");
                }
                _logger.LogInformation($"Вычисление хеша пароля");
                var hashpaswd = _hashService.HashString(newPassword);
                _logger.LogInformation($"Вычисление хеша пароля успешно");
                var user = await _repository.GetUserByLoginAsync(login);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} не найден");
                    throw new UserNotExistsException($"Пользователь с логином: {login} не найден");
                }
                user.Password = hashpaswd;
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = tokendata.Login;
                await _repository.SaveChangesAsync();
                _logger.LogInformation($"Попытка поменять пароль у пользователя с логином: {login} прошла успешно");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке поменять пароль у пользователя с логином: {login}. Ошибка: {ex.Message}");
                throw;
            }
        }
        public async Task ChangeUserLoginAsync(string token, string oldlogin, string login)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка изменить логин пользователя с логином: {oldlogin}");
                var user = await _repository.GetUserByLoginAsync(oldlogin);
                var existuser = await _repository.GetUserByLoginAsync(login);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином: {oldlogin} не найден");
                    throw new UserNotExistsException($"Пользователь с логином: {oldlogin} не найден");
                }
                if (!Regex.IsMatch(login, "^[a-zA-Z0-9]+$"))
                {
                    _logger.LogWarning($"Неверный формат логина. Только латинские буквы и цифры.");
                    throw new WrongDataException("Неверный формат логина. Только латинские буквы и цифры.");
                }
                if (user.Login == login)
                {
                    _logger.LogInformation($"Логин пользователя с логином: {oldlogin} уже имеет указанный для изменения логин: {login}, изменений не требуется");
                    return;
                }
                if (existuser != null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} уже существует");
                    throw new UserExistException($"Пользователь с логином: {login} уже существует");
                }
                user.Login = login;
                user.ModifiedOn= DateTime.UtcNow;
                user.ModifiedBy = tokendata.Login;
                await _repository.SaveChangesAsync();
                _logger.LogInformation($"Попытка изменить логин пользователя с логином: {oldlogin} прошла успешно");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке изменить логин пользователя с логином: {oldlogin}. Ошибка: {ex.Message}");
                throw;
            }
        }
        public async Task<List<User>> GetAllActiveUsersAsync(string token)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка получить всех активных пользователей");
                var users = await _repository.GetAllActiveUsersAsync();
                _logger.LogInformation($"Попытка получить всех активных пользователей прошла успешно!");
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке получить всех активных пользователей. Ошибка: {ex.Message}");
                throw;
            }
        }
        public async Task<List<User>> GetAllUsersOlderThanDataAsync(string token, int years)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка получить всех пользователей старше {years}");
                if (years <= 0)
                {
                    _logger.LogInformation($"Введен неправильный формат возраста на вход.");
                    throw new WrongDataException("Неправильный формат возраста на вход. Число должно быть положительным.");
                }
                var users = await _repository.GetAllOlderASync(years);
                _logger.LogInformation($"Попытка получить всех пользователей старше {years} прошла успешно!");
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке получить всех пользователей старше {years} Ошибка: {ex.Message}");
                throw;
            }
        }
        public async Task RemoveUserSoftlyByLoginAsync(string token, string login)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка удалить пользователя с логином: {login}");
                var user = await _repository.GetUserByLoginAsync(login);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} не найден");
                    throw new UserNotExistsException($"Пользователь с логином: {login} не найден");
                }
                await _repository.DeleteUserAsync(user, tokendata.Login);
                _logger.LogInformation($"Попытка удалить пользователя с логином: {login} прошла успешно!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке удалить пользователя с логином: {login}. Ошибка: {ex.Message}");
                throw;
            }
        }
        public async Task RestoreUserByLoginAsync(string token, string login)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка восстановить пользователя с логином: {login}");
                var user = await _repository.GetUserByLoginAsync(login);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} не найден");
                    throw new UserNotExistsException($"Пользователь с логином: {login} не найден");
                }
                user.RevokedBy = null;
                user.RevokedOn = null;
                await _repository.SaveChangesAsync();
                _logger.LogInformation($"Попытка восстановить  пользователя с логином: {login} прошла успешно!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке восстановить  пользователя с логином: {login}. Ошибка: {ex.Message}");
                throw;
            }
        }
    }
}
