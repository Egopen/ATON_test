using System.ComponentModel.DataAnnotations;

namespace UserApp.UI.DTO
{
    public class AuthDTO
    {
        [Required]
        public string Login {  get; set; }
        [Required]
        public string Password { get; set; }
    }
}
