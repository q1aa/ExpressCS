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
    }
}
