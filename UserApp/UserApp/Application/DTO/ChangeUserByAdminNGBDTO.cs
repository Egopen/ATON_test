namespace UserApp.Application.DTO
{
    public class ChangeUserByAdminNGBDTO
    {
        public string Token { get; set; }
        public string Login { get; set; }
        public string? Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
