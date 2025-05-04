using System.ComponentModel.DataAnnotations;

namespace UserApp.UI.DTO
{
    public class ChangeUserByAdminUINGBDTO
    {
        [Required]
        public string Login { get; set; }
        public string? Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
