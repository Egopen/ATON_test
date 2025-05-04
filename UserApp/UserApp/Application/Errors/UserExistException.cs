namespace UserApp.Application.Errors
{
    public class UserExistException:Exception
    {
        public UserExistException() : base() { }
        public UserExistException(string mes) : base(mes) { }
    }
}
