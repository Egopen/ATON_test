using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using UserApp.Application.DTO;
using UserApp.Application.Errors;
using UserApp.Application.Services.Implementations;
using UserApp.Application.Services.Interfaces;
using UserApp.Infrastructure.DB.Models;
using UserApp.UI.DTO;

namespace UserApp.UI.Controllers
{
    [Route("UserApp/[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _logger = logger;
            _adminService = adminService;
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
        private IActionResult ProcessException(Exception exception) {
            if (exception is InvalidTokenException)
            {
                _logger.LogWarning("Неправильные данные для доступа к методу.");
                return Unauthorized("Неправильные данные для доступа к методу.");
            }
            else if (exception is UnauthorizedAccessException) {
                _logger.LogWarning("Токен не предоставлен.");
                return Unauthorized("Токен не предоставлен.");
            }
            else if (exception is UserNotExistsException)
            {
                _logger.LogWarning("Пользователь не найден.");
                return BadRequest("Пользователь не найден.");
            }
            else if (exception is UserNotAdminException)
            {
                _logger.LogWarning("У вас нету прав администратора");
                return Forbid("Неправильные данные для доступа к методу.");
            }
            else if (exception is WrongDataException)
            {
                _logger.LogWarning($"Неправильный формат данных. {exception.Message}");
                return BadRequest($"Неправильный формат данных. {exception.Message}");
            }
            else if (exception is UserExistException)
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
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUserById([Required, FromQuery] Guid id)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу полчуения пользователя по айди");
                var token = GetToken();
                var user = await _adminService.GetUserByIdAsync(token, id);
                _logger.LogInformation("обращение к апи-методу полчуения пользователя по айди успешно");
                return Ok(user);
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUserByLogin([Required, FromQuery] string login)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу полчуения пользователя по логину");
                var token = GetToken();
                var user = await _adminService.GetUserByLoginAsync(token, login);
                _logger.LogInformation("обращение к апи-методу полчуения пользователя по логину успешно");
                return Ok(user);
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([Required] CreateUserUIDTO user)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу создания пользователя");
                var token = GetToken();
                var data = new CreateUserDTO
                {
                    Token = token,
                    Login = user.Login,
                    Password = user.Password,
                    Name = user.Name,
                    Birthday = user.Birthday,
                    Gender = user.Gender,
                    Admin = user.Admin
                };
                await _adminService.CreateUserAsync(data);
                _logger.LogInformation("обращение к апи-методу создания пользователя успешно");
                return Ok("Пользователь успешно создан");
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> ChangeUserPassword([Required]ChangeUserPasswordDTO changes)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу изменения пароля пользователя");
                var token = GetToken();
                await _adminService.ChangeUserPasswordByAdminAsync(token, changes.Login, changes.NewPassword);
                _logger.LogInformation("обращение к апи-методу создания пользователя успешно");
                return Ok("Пароль успешно изменен");
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> ChangeUserNameGenderBirthday(ChangeUserByAdminUINGBDTO changes)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу изменения имени, гендера, даты рождения");
                var token = GetToken();
                var data = new ChangeUserByAdminNGBDTO
                {
                    Token = token,
                    Gender = changes.Gender,
                    Name = changes.Name,
                    Login= changes.Login,
                    Birthday = changes.Birthday,
                };
                await _adminService.ChangeUserNameGenderBirthdayAsync(data);
                _logger.LogInformation("обращение к апи-методу изменения имени, гендера, даты рождения успешно");
                return Ok("Данные успешно изменены");
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> ChangeUserLogin([Required] ChangeUserLoginDTO changes)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу изменения логина");
                var token = GetToken();
                await _adminService.ChangeUserLoginAsync(token, changes.OldLogin, changes.NewLogin);
                _logger.LogInformation("обращение к апи-методу изменения логина успешно");
                return Ok("Данные успешно изменены");
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllActiveUsers()
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу полученяи всех активных пользователей");
                var token = GetToken();
                var users=await _adminService.GetAllActiveUsersAsync(token);
                _logger.LogInformation("обращение к апи-методу полученяи всех активных пользователей");
                return Ok(users);
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsersOlderThanData([Required, FromQuery] int years)
        {
            try
            {
                _logger.LogInformation($"обращение к апи-методу получения всех пользователей старше {years} ");
                var token = GetToken();
                var users=await _adminService.GetAllUsersOlderThanDataAsync(token,years);
                _logger.LogInformation($"обращение к апи-методу получения всех пользователей старше {years}  успешно");
                return Ok(users);
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> RemoveUserSoftlyByLogin([Required]string login)
        {
            try
            {
                _logger.LogInformation($"обращение к апи-методу удаления пользователя");
                var token = GetToken();
                await _adminService.RemoveUserSoftlyByLoginAsync(token,login);
                _logger.LogInformation($"обращение к апи-методу удаления пользователя  успешно");
                return Ok("Пользователь успешно удален.");
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
        [Authorize(AuthenticationSchemes = "Access", Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RestoreUserByLogin([Required]string login)
        {
            try
            {
                _logger.LogInformation($"обращение к апи-методу восстановления пользователя");
                var token = GetToken();
                await _adminService.RestoreUserByLoginAsync(token, login);
                _logger.LogInformation($"обращение к апи-методу восстановления пользователя успешно");
                return Ok("Пользователь успешно восстановлен.");
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }
    }
}
