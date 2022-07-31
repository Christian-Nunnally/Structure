using System;

namespace Structur.Program.Utilities
{
    public static class Executor
    {
        public static bool ThrowExceptions { get; set; }

        public static void SafelyExecute(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (ThrowExceptions) throw new InvalidProgramException("Exception" + e.Message, e);
            }
        }
    }
}
