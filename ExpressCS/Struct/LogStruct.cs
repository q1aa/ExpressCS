using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Types
{
    public struct LogStruct
    {
        public string Message { get; set; }
        public ConsoleColor Color { get; set; }

        public LogStruct(string message, ConsoleColor color)
        {
            Message = message;
            Color = color;
        }
    }
}
