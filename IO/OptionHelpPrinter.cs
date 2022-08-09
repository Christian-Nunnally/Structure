using Structur.IO;
using Structur.Program.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structure.IO
{
    internal class OptionHelpPrinter
    {
        private readonly StructureIO _io;

        public OptionHelpPrinter(StructureIO io)
        {
            _io = io;
        }

        public void PrintHelp(string helpString, Dictionary<ConsoleKeyInfo, UserAction> keyedOptions)
        {
            if (string.IsNullOrEmpty(helpString)) keyedOptions.All(PrintOption);
            else _io.Write(helpString);
        }

        private void PrintOption(KeyValuePair<ConsoleKeyInfo, UserAction> x) => _io.WriteNoLine(OptionString(x.Key, x.Value));

        private static string OptionString(ConsoleKeyInfo key, UserAction option)
        {
            return !string.IsNullOrEmpty(option.Description) 
                ? $" {Utility.KeyToKeyString(key)} - {option.Description}\n" 
                : string.Empty;
        }
    }
}
