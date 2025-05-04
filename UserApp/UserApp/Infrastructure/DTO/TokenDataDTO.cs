namespace UserApp.Infrastructure.DTO
{
    public class TokenDataDTO
    {
        public Guid Guid { get; set; }
        public string Login {  get; set; }
        public bool isAdmin { get; set; }
    }
}
