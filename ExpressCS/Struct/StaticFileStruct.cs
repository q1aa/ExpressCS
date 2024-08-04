using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Struct
{
    public struct StaticFileStruct
    {
        public string WebPath { get; set; }
        public DirectoryInfo DirectoryPath { get; set; }

        public StaticFileStruct(string webPath, DirectoryInfo directoryPath)
        {
            WebPath = webPath;
            DirectoryPath = directoryPath;
        }
    }
}
