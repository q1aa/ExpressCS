using ExpressCS.Struct;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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

        public static NameValueCollection getDynamicParamsFromURL(string routeURL, string browserURL)
        {
            if (browserURL.EndsWith("/")) browserURL = browserURL.Remove(browserURL.Length - 1);

            string[] routePath = routeURL.Split('/');
            string[] reqPath = browserURL.Split('/');

            NameValueCollection dynamicParams = new NameValueCollection();

            for (int i = 0; i < routePath.Length; i++)
            {
                if (routePath[i].StartsWith(":"))
                {
                    dynamicParams.Add(routePath[i].Substring(1), reqPath[i]);
                }
            }

            if (dynamicParams.Count == 0) return new NameValueCollection();
            return dynamicParams;
        }

        public static NameValueCollection getQueryParamsFromURL(string url)
        {
            NameValueCollection queryParams = new NameValueCollection();
            if (!url.Contains("?")) return queryParams;

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

        public static NameValueCollection parseJSONBody(string? body)
        {
            NameValueCollection jsonBody = new NameValueCollection();
            try
            {
                while (body != null)
                {
                    JsonNode jsonNode = JsonNode.Parse(body);
                    if (jsonNode == null) break;

                    if (jsonNode is JsonObject jsonObject)
                    {
                        foreach (KeyValuePair<string, JsonNode> jsonPair in jsonObject)
                        {
                            jsonBody.Add(jsonPair.Key, jsonPair.Value.ToString());
                        }
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
            }

            return jsonBody;
        }

        public static NameValueCollection parseFormDataBody(string? body, string? boundary)
        {
            NameValueCollection formData = new NameValueCollection();
            if(boundary == null || body == null) return formData;

            body = body.Replace("--" + boundary + "--", "");
            string[] parts = body.Split(new string[] { "--" + boundary }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string[] lines = part.Split("\r\n").Skip(1).SkipLast(1).ToArray();
                string key = lines[0].Trim().Split("name=\"")[1].Split("\"")[0];
                string[] value = lines.Skip(2).ToArray();

                formData.Add(key, string.Join("\r\n", value));
            }

            return formData;
        }

        public static Stream CopyInputStream(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            stream.Close();
            return ms;
        }
    }
}
