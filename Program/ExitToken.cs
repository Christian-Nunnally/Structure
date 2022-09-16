namespace Structur.Program
{
    public class ExitToken
    {
        public static ExitToken ExitingToken { get; } = new() { Exit=true };

        public bool Exit { get; set; }
    }
}