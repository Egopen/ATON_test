namespace UserApp.Application.Errors
{
    public class UserRevokedException:Exception
    {
        public UserRevokedException() : base() { }
        public UserRevokedException(string mes) : base(mes) { }
    }
}
