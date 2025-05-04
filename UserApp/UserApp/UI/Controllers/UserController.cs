using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using UserApp.Application.DTO;
using UserApp.Application.Errors;
using UserApp.Application.Services.Interfaces;
using UserApp.UI.DTO;

namespace UserApp.UI.Controllers
{
    [Route("UserApp/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _logger = logger;
            _userService = userService;
        }
        private string GetToken()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                throw new UnauthorizedAccessException("токен не предоставлен");
            }
            var token = authHeader.StartsWith("Bearer ")
                ? authHeader.Substring("Bearer ".Length)
                : authHeader;
            return token;
        }
        private IActionResult ProcessException(Exception exception)
        {
            if (exception is InvalidTokenException)
            {
                _logger.LogWarning("Неправильные данные для доступа к методу.");
                return Unauthorized("Неправильные данные для доступа к методу.");
            }
            else if (exception is UnauthorizedAccessException)
            {
                _logger.LogWarning("Токен не предоставлен.");
                return Unauthorized("Токен не предоставлен.");
            }
            else if (exception is UserNotExistsException)
            {
                _logger.LogWarning("Пользователь не найден.");
                return BadRequest("Пользователь не найден.");
            }
            else if (exception is UserRevokedException)
            {
                _logger.LogWarning($"Пользователь удален, у него нет прав оращатся к методам.");
                return Unauthorized($"Извинити, вас уже удалил из системы, обратитесь к админстрации сайта, чтобы возобновить аккаунт.");
            }
            else if (exception is WrongDataException)
            {
                _logger.LogWarning($"Неправильный формат данных. {exception.Message}");
                return BadRequest($"Неправильный формат данных. {exception.Message}");
            }
            else if (exception is UserExistException )
            {
                _logger.LogWarning($"Пользователь с таким логином уже существует");
                return BadRequest($"Пользователь с таким логином уже существует");
            }
            else
            {
                _logger.LogWarning($"Произошла ошибка в апи-методе, ошибка на стороне сервера. Ошибка:{exception.Message}");
                return StatusCode(500, "Что-то пошло не так");
            }

        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPut]
        public async Task<IActionResult> ChangeUserNameGenderBirthday([Required] ChangeUserNGBUIDTO changes)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу изменения имени, гендера, даты рождения");
                var token = GetToken();
                var data = new ChangeUserNGBDTO
                {
                    Token = token,
                    Birthday = changes.Birthday,
                    Gender = changes.Gender,
                    Name = changes.Name
                };
                await _userService.ChangeUserNameGenderBirthdayAsync(data);
                _logger.LogInformation("обращение к апи-методу изменения имени, гендера, даты рождения успешно");
                return Ok("Данные успешно изменены");

            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPut]
        public async Task<IActionResult> ChangeUserPassword([Required]ChangeUserPasswordByUserDTO changes)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу изменения пароля.");
                var token = GetToken();
                await _userService.ChangeUserPasswordByUserAsync(token, changes.OldPassword, changes.NewPassword);
                _logger.LogInformation("обращение к апи-методу изменения пароля успешно.");
                return Ok("Данные успешно изменены");
            }
            catch (WrongPasswordException ex)
            {
                _logger.LogWarning($"Страрый пароль указнный пользователем неверный.");
                return BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPut]
        public async Task<IActionResult> ChangeUserLogin([Required] ChangeUserrLoginByUserDTO changes)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу изменения логина");
                var token = GetToken();
                await _userService.ChangeUserLoginAsync(token,changes.NewLogin);
                _logger.LogInformation("обращение к апи-методу изменения  логина успешно");
                return Ok("Данные успешно изменены");

            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> GetUserData([Required]GetUserDataDTO data)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу получения данных пользователя");
                var token = GetToken();
                var user = await _userService.GetUserByUserDataAsync(token,data.Login,data.Password); ;
                _logger.LogInformation("обращение к апи-методу получения данных пользователя успешно");
                return Ok(user);

            }
            catch (WrongPasswordException ex)
            {
                _logger.LogWarning($"Пароль указнный пользователем неверный.");
                return BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }

    }
}
