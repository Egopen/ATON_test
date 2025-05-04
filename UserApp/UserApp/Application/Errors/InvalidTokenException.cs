namespace UserApp.Application.Errors
{
    public class InvalidTokenException:Exception
    {
        public InvalidTokenException() : base() { }
        public InvalidTokenException(string mes) : base(mes) { }
    }
}
