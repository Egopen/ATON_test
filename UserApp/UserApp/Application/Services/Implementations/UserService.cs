using System.Text.RegularExpressions;
using System.Threading.Channels;
using UserApp.Application.DTO;
using UserApp.Application.Errors;
using UserApp.Application.Services.Interfaces;
using UserApp.Infrastructure.DB.Models;
using UserApp.Infrastructure.DTO;
using UserApp.Infrastructure.Services.Interfaces;

namespace UserApp.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly IHashService _hashService;
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository repository, ILogger<UserService> logger, IHashService hashService,ITokenService tokenService)
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
            return _tokenService.GetTokenData(token);
        }
        public async Task ChangeUserNameGenderBirthdayAsync(ChangeUserNGBDTO changes)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(changes.Token);
                _logger.LogInformation($"Попытка поменять имя или др или пол у пользователя с ID: {tokendata.Guid}");
                var user = await _repository.GetUserByIdAsync(tokendata.Guid);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с ID: {tokendata.Guid} не найден");
                    throw new UserNotExistsException($"Пользователь с ID: {tokendata.Guid} не найден");
                }
                if (user.RevokedOn != null)
                {
                    _logger.LogWarning($"Пользователь с ID: {tokendata.Guid} уже удален");
                    throw new UserRevokedException($"Пользователь с ID: {tokendata.Guid} уже удален");
                }
                var isUpdated = false;
                if (!string.IsNullOrEmpty(changes.Name) && user.Name != changes.Name)
                {
                    if (!Regex.IsMatch(changes.Name, "^^[a-zA-Zа-яА-ЯёЁ\\s]+$"))
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
                _logger.LogInformation($"Попытка поменять имя или др или пол у пользователя с ID: {tokendata.Guid} прошла успешно");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке поменять имя или др или пол у пользователя. Ошибка: {ex.Message}");
                throw;
            }

        }

        public async Task ChangeUserPasswordByUserAsync(string token, string oldPassword, string newPassword)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Данные из токена получены");
                _logger.LogInformation($"Попытка поменять свой пароль пользователем с ID: {tokendata.Guid}");
                var user = await _repository.GetUserByIdAsync(tokendata.Guid);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с ID: {tokendata.Guid} не найден");
                    throw new UserNotExistsException($"Пользователь с ID: {tokendata.Guid} не найден");
                }
                _logger.LogInformation($"Попытка поменять пароль у пользователя с ID: {tokendata.Guid}");
                if (!Regex.IsMatch(newPassword, "^[a-zA-Z0-9]+$") || newPassword.Length < 8)
                {
                    _logger.LogWarning("Введен неверный формат пароля");
                    throw new WrongDataException("Введен неверный формат пароля");
                }
                if (user.RevokedOn != null)
                {
                    _logger.LogWarning($"Пользователь с ID: {tokendata.Guid} уже удален");
                    throw new UserRevokedException($"Пользователь с ID: {tokendata.Guid} уже удален");
                }
                _logger.LogInformation($"Вычисление хеша нового пароля");
                var hashpaswd = _hashService.HashString(newPassword);
                _logger.LogInformation($"Вычисление хеша нового пароля успешно");
                _logger.LogInformation($"Вычисление хеша нового пароля");
                var oldhashpaswd = _hashService.HashString(oldPassword);
                _logger.LogInformation($"Вычисление хеша нового пароля успешно");
                if (user.Password != oldhashpaswd)
                {
                    _logger.LogWarning($"Неверный старый пароль для пользователя с ID: {tokendata.Guid}");
                    throw new WrongPasswordException("Неверный старый пароль для пользователя");
                }
                user.Password = hashpaswd;
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = tokendata.Login;
                await _repository.SaveChangesAsync();
                _logger.LogInformation($"Попытка поменять свой пароль пользователем с ID: {tokendata.Guid} прошла успешно");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке поменять свой пароль пользователем. Ошибка: {ex.Message}");
                throw;
            }
        }
        public async Task ChangeUserLoginAsync(string token, string login)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка изменить логин пользователя с ID: {tokendata.Guid}");
                var user = await _repository.GetUserByIdAsync(tokendata.Guid);
                var existuser = await _repository.GetUserByLoginAsync(login);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с ID: {tokendata.Guid} не найден");
                    throw new UserNotExistsException($"Пользователь с ID: {tokendata.Guid} не найден");
                }
                if (user.RevokedOn != null)
                {
                    _logger.LogWarning($"Пользователь с ID: {tokendata.Guid} уже удален");
                    throw new UserRevokedException($"Пользователь с ID: {tokendata.Guid} уже удален");
                }
                if (!Regex.IsMatch(login, "^[a-zA-Z0-9]+$"))
                {
                    throw new WrongDataException("Неверный формат логина.");
                }
                if (user.Login == login)
                {
                    _logger.LogInformation($"Логин пользователя с ID: {tokendata.Guid} уже равен {login}, изменений не требуется");
                    return;
                }
                if (existuser != null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} уже существует");
                    throw new UserExistException($"Пользователь с логином: {login} уже существует");
                }
                user.Login = login;
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = tokendata.Login;
                await _repository.SaveChangesAsync();
                _logger.LogInformation($"Попытка изменить логин пользователя с ID: {tokendata.Guid} прошла успешно");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке изменить логин пользователя. Ошибка: {ex.Message}");
                throw;
            }
        }

        public async Task<GetUserDataByUserDTO> GetUserByUserDataAsync(string token,string login, string password)
        {
            try
            {
                var tokendata = ValidateAndGetTokenData(token);
                _logger.LogInformation($"Попытка получить данные пользователся с логином: {login}");
                var user = await _repository.GetUserByLoginAsync(login);
                var hashpswd = _hashService.HashString(password);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} не найден");
                    throw new UserNotExistsException($"Пользователь с логином: {login} не найден");
                }
                if (user.RevokedOn != null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} уже удален");
                    throw new UserRevokedException($"Пользователь с логином:  {login} уже удален");
                }
                if (user.Password != hashpswd)
                {
                    _logger.LogWarning($"Неверный пароль для пользователя с логином: {login}");
                    throw new WrongPasswordException("Неверный пароль для пользователя");
                }
                _logger.LogInformation($"Попытка получить данные пользователся с логином: {login} прошлоа успшно!");
                return new GetUserDataByUserDTO
                {
                    Name=user.Name,
                    Login=user.Login,
                    Birthday=user.Birthday,
                    CreatedOn=user.CreatedOn,
                    Gender=user.Gender
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке получить данные пользователся с логином: {login}. Ошибка: {ex.Message}");
                throw;
            }
        }

    }
}
