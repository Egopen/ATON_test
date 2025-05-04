namespace UserApp.Application.Errors
{
    public class WrongDataException:Exception
    {
        public WrongDataException() : base() { }
        public WrongDataException(string mes) : base(mes) { }
    }
}
