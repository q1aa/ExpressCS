using ExpressCS.Struct;
using ExpressCS.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Utils
{
    public class LogUtil
    {
        private static void WriteLineWithColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Log(string message, ConsoleColor color = ConsoleColor.White, bool prefix = true)
        {
           if(prefix) WriteLineWithColor($"[ExpressCS] {message}", color);
           else WriteLineWithColor(message, color);
        }

        public static void LogError(string message)
        {
            Log(message, ConsoleColor.Red);
        }

        public static void LogWarning(string message)
        {
            Log(message, ConsoleColor.Yellow);
        }

        public static void LogRouteRegister(string path, Struct.HttpMethod[] method, bool webSocketRoute = false)
        {
            Console.ForegroundColor = ConsoleColor.White;
            
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"{path} ");

            foreach (var methode in method)
            {
                Console.ForegroundColor = HelperUtil.getHttpMethodeColor(methode);
                Console.Write($"{methode} ");
            }

            Console.WriteLine();
            Console.ResetColor();
        }
        
        public static void LogPublicDirectory(string path, int fileCount = 0, ConsoleColor color = ConsoleColor.Green)
        {
            Log($"Public directory registered: {path}, {fileCount} files", color);
        }
    }
}
