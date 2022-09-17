using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structur.Server.Requests
{
    public class HistoryRequest
    {
        public int StartInclusive { get; set; }

        public int StopExclusive { get; set; }
    }
}
