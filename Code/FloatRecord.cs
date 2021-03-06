using System;

namespace Structure
{
    public class FloatRecord
    {
        public float Value;
        public DateTime Date = DateTime.Now;
        public string Description;

        public override string ToString()
        {
            return $"[{Date.ToString("yyyy-MM-dd:HH:mm:ss")}] {Description} = {Value}";
        }
    }
}