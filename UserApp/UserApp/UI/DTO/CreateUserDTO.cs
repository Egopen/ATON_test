using System.ComponentModel.DataAnnotations;

namespace UserApp.UI.DTO
{
    public class CreateUserUIDTO
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        [Required]
        public bool Admin { get; set; }
    }
}
