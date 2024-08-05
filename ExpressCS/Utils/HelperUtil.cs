using ExpressCS.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Utils
{
    internal class HelperUtil
    {
        public static Struct.HttpMethod convertRequestMethode(string requestMethode)
        {
            return requestMethode switch
            {
                "GET" => Struct.HttpMethod.GET,
                "POST" => Struct.HttpMethod.POST,
                "PUT" => Struct.HttpMethod.PUT,
                "DELETE" => Struct.HttpMethod.DELETE,
                "PATCH" => Struct.HttpMethod.PATCH,
                "OPTIONS" => Struct.HttpMethod.OPTIONS,
                "HEAD" => Struct.HttpMethod.HEAD,
                "ANY" => Struct.HttpMethod.ANY,
                _ => Struct.HttpMethod.ANY
            };
        }

        public static string[] getDynamicParamsFromURL(RouteStruct route, string url)
        {
            string[] routePath = route.Path.Split('/');
            string[] reqPath = url.Split('/');

            List<string> dynamicParams = new List<string>();

            for (int i = 0; i < routePath.Length; i++)
            {
                if (routePath[i].StartsWith(":"))
                {
                    dynamicParams.Add(reqPath[i]);
                }
            }

            if (dynamicParams.Count == 0) return null;
            return dynamicParams.ToArray();
        }

        public static Dictionary<string, string> getQueryParamsFromURL(string url)
        {
            if (!url.Contains("?")) return null;
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            foreach (string query in url.Split('?')[1].Split('&'))
            {
                string[] queryParts = query.Split('=');
                queryParams.Add(queryParts[0], queryParts[1]);
            }
            return queryParams;
        }

        public static int getStatusCode(int setStatusCode, int preferredStatusCode)
        {
            return setStatusCode == -1 ? preferredStatusCode : setStatusCode;
        }

        public static string getContentType(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".html":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "text/javascript";
                case ".json":
                    return "application/json";
                case ".png":
                    return "image/png";
                case ".jpg":
                    return "image/jpeg";
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".svg":
                    return "image/svg+xml";
                case ".ico":
                    return "image/x-icon";
                case ".xml":
                    return "application/xml";
                case ".pdf":
                    return "application/pdf";
                case ".zip":
                    return "application/zip";
                case ".mp3":
                    return "audio/mpeg";
                case ".mp4":
                    return "video/mp4";
                case ".webm":
                    return "video/webm";
                case ".ogg":
                    return "audio/ogg";
                case ".wav":
                    return "audio/wav";
                case ".webp":
                    return "image/webp";
                case ".csv":
                    return "text/csv";
                case ".txt":
                    return "text/plain";
                default:
                    return "application/octet-stream";
            }
        }

        public static ConsoleColor getHttpMethodeColor(Struct.HttpMethod httpMethod)
        {
            return httpMethod switch
            {
                Struct.HttpMethod.GET => ConsoleColor.Green,
                Struct.HttpMethod.POST => ConsoleColor.Blue,
                Struct.HttpMethod.PUT => ConsoleColor.Yellow,
                Struct.HttpMethod.DELETE => ConsoleColor.Red,
                Struct.HttpMethod.PATCH => ConsoleColor.Magenta,
                Struct.HttpMethod.OPTIONS => ConsoleColor.Cyan,
                Struct.HttpMethod.HEAD => ConsoleColor.DarkYellow,
                Struct.HttpMethod.ANY => ConsoleColor.DarkMagenta,
                _ => ConsoleColor.White
            };
        }
    }
}
