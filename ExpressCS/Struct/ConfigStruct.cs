﻿namespace ExpressCS.Types
{
    public struct ConfigStruct
    {
        public int Port { get; }
        public string Host { get; }
        public bool Ssl { get; }
        public bool ShowTransferedDataSize { get; set; } = true;
        public bool IgnoreWriteExceptions { get; set; } = true;
        public int MaxRequestSizeInBytes { get; set; }
        internal ConfigStruct(int port, string host, bool ssl, bool showTransferedDataSize, bool ignoreWriteExceptions, int maxRequestSizeInBytes)
        {
            Port = port;
            Host = host;
            Ssl = ssl;
            ShowTransferedDataSize = showTransferedDataSize;
            IgnoreWriteExceptions = ignoreWriteExceptions;
            MaxRequestSizeInBytes = maxRequestSizeInBytes;
        }
    }
}
