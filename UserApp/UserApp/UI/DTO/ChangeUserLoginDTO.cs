using System.ComponentModel.DataAnnotations;

namespace UserApp.UI.DTO
{
    public class ChangeUserLoginDTO
    {
        [Required]
        public string OldLogin {  get; set; }
        [Required]
        public string NewLogin { get; set; }
    }
}
