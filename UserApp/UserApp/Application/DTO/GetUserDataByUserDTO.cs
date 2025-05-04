namespace UserApp.Application.DTO
{
    public class GetUserDataByUserDTO
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
