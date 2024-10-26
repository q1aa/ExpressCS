using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Types
{
    public struct ConfigStruct
    {
        public int Port { get; }
        public string Host { get; }
        public bool Ssl { get; }

        public bool ShowTransferedDataSize { get; set; } = true;

        internal ConfigStruct(int port, string host, bool ssl, bool showTransferedDataSize)
        {
            Port = port;
            Host = host;
            Ssl = ssl;
            ShowTransferedDataSize = showTransferedDataSize;
        }
    }
}
