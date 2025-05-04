using System.ComponentModel.DataAnnotations;

namespace UserApp.UI.DTO
{
    public class ChangeUserPasswordDTO
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
