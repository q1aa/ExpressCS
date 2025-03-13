using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Struct
{
    public class ReceiveFileStruct
    {
        public void Dispose()
        {
            Data.Dispose();
        }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public MemoryStream Data { get; set; }
    }
}
