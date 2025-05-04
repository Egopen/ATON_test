using System.ComponentModel.DataAnnotations;

namespace UserApp.UI.DTO
{
    public class ChangeUserPasswordByUserDTO
    {
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
