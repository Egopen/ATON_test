using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using UserApp.Application.Errors;
using UserApp.Application.Services.Interfaces;
using UserApp.UI.DTO;

namespace UserApp.UI.Controllers
{
    [Route("UserApp/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _logger = logger;
            _authService = authService;
        }
        [HttpPost]
        public async Task<IActionResult> SignIn([Required] AuthDTO data)
        {
            try
            {
                _logger.LogInformation("обращение к апи-методу входа");
                var token = await _authService.SignInAsync(data.Login, data.Password);
                _logger.LogInformation("обращение к апи-методу входа успешно");
                return Ok(new
                {
                    Accesstoken = token
                });
            }
            catch (UserNotExistsException ex)
            {
                _logger.LogWarning("Произошла ошибка в апи-методе входа, неправильные данные или пользователь удален.");
                return Unauthorized("Неправильный логин или пароль");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Произошла ошибка в апи-методе входа, ошибка на стороне ссервера.");
                return StatusCode(500, "Что-то пошло не так");
            }
        }
    }
}
