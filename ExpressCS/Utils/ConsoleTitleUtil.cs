using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Utils
{
    internal class ConsoleTitleUtil
    {
        public static void ChangeTitle(string title)
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Console.Title = title;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Console.WriteLine($"\u001b]2;{title}\u0007");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Console.WriteLine($"\u001b]2;{title}\u0007");
        }
    }
}
