namespace UserApp.Application.DTO
{
    public class ChangeUserNGBDTO
    {
        public string Token { get; set; }
        public string? Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
