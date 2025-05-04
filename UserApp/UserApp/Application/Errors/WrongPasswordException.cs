namespace UserApp.Application.Errors
{
    public class WrongPasswordException:Exception
    {
        public WrongPasswordException() : base() { }
        public WrongPasswordException(string mes) : base(mes) { }
    }
}
