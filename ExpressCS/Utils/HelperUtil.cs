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
    }
}
