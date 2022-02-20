using System;

namespace Structure.Structure.Utility
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
                if (ThrowExceptions) throw new Exception("Exception" + e.Message, e);
            }
        }
    }
}
