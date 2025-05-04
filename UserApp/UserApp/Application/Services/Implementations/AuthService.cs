using UserApp.Application.DTO;
using UserApp.Application.Errors;
using UserApp.Application.Services.Interfaces;
using UserApp.Infrastructure.Services.Interfaces;

namespace UserApp.Application.Services.Implementations
{
    public class AuthService:IAuthService
    {
        private readonly IUserRepository _repository;
        private readonly ITokenService _tokenService;
        private readonly IHashService _hashService;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IUserRepository userRepository,ITokenService tokenService,IHashService hashService, ILogger<AuthService> logger)
        {
            _tokenService = tokenService;
            _repository = userRepository;
            _hashService = hashService;
            _logger = logger;
        }
        public async Task<string> SignInAsync(string login, string password)
        {
            try
            {
                _logger.LogInformation($"Попытка авторизоваться пользователем с логином: {login}");
                var user = await _repository.GetUserByLoginAsync(login);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} не найден");
                    throw new UserNotExistsException($"Пользователь с логином: {login} не найден");
                }
                if(user.RevokedOn != null)
                {
                    _logger.LogWarning($"Пользователь с логином: {login} удален");
                    throw new UserNotExistsException($"Пользователь с логином: {login} удален");
                }
                var hashPasswd = _hashService.HashString(password);
                if (user.Password != hashPasswd) {
                    throw new UserNotExistsException($"Неправильный пароль для пользователся с логином: {login}");
                }
                var token=_tokenService.CreateTokenAsync(user.Guid,user.Login,user.Admin);
                _logger.LogInformation($"Пользователь авторизовался с логином: {login}!");
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Произошла ошибка при попытке авторизоваться пользователем с логином: {login}");
                throw;
            }
        }
    }
}
