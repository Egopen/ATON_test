namespace UserApp.Application.Errors
{
    public class UserNotAdminException:Exception
    {
        public UserNotAdminException() : base() { }
        public UserNotAdminException(string mes) : base(mes) { }
    }
}
