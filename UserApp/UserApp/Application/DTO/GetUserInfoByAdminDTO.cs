namespace UserApp.Application.DTO
{
    public class GetUserInfoByAdminDTO
    {
        public string Token { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool IsActive { get; set; }
    }
}
