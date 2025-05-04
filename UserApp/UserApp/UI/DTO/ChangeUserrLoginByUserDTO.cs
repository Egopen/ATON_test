using System.ComponentModel.DataAnnotations;

namespace UserApp.UI.DTO
{
    public class ChangeUserrLoginByUserDTO
    {
        [Required]
        public string NewLogin {  get; set; }
    }
}
