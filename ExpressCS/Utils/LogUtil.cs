using ExpressCS.Struct;
using ExpressCS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Utils
{
    public class LogUtil
    {
        public static void WriteLineWithColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Log(string message, ConsoleColor color = ConsoleColor.White)
        {
           WriteLineWithColor($"[ExpressCS] {message}", color);
        }

        public static void LogError(string message)
        {
            Log(message, ConsoleColor.Red);
        }

        public static void LogWarning(string message)
        {
            Log(message, ConsoleColor.Yellow);
        }

        public static void LogInfo(string message)
        {
            Log(message, ConsoleColor.Cyan);
        }

        public static void LogRouteRegister(string path, Struct.HttpMethod[] method, bool webSocketRoute = false)
        {
            Console.ForegroundColor = ConsoleColor.White;
            if(!webSocketRoute) Console.Write($"[ExpressCS] Route registered: ");
            else Console.Write($"[ExpressCS] WebSocket route registered: ");
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
    }
}
