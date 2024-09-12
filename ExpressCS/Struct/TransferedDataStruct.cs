using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Struct
{
    class TransferedDataStruct
    {
        public long UploadSize { get; set; }
        public TransferedUnit UploadUnit { get; set; }
        public long DownloadSize { get; set; }
        public TransferedUnit DownloadUnit { get; set; }

        public TransferedDataStruct(long uploadSize, TransferedUnit uploadUnit, long downloadSize, TransferedUnit downloadUnit)
        {
            UploadSize = uploadSize;
            UploadUnit = uploadUnit;
            DownloadSize = downloadSize;
            DownloadUnit = downloadUnit;
        }
    }
    enum TransferedUnit
    {
        B,
        KB,
        MB,
    }
}
