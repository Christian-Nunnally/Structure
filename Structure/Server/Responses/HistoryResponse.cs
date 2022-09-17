using Structur.IO.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structur.Server.Responses
{
    public class HistoryResponse
    {
        public static HistoryResponse Create(ProgramInputData[] history)
        {
            return new HistoryResponse
            {
                History = history,
            };
        }

        public ProgramInputData[] History { get; set; }
    }
}
