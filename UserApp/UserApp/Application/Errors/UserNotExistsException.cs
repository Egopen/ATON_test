namespace UserApp.Application.Errors
{
    public class UserNotExistsException:Exception
    {
        public UserNotExistsException() : base() { }
        public UserNotExistsException(string mes):base(mes) { }
    }
}
