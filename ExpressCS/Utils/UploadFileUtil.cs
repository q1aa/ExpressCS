using ExpressCS.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestHttpListenerContext;

namespace ExpressCS.Utils
{
    internal class UploadFileUtil
    {
        public static async Task<ReceiveFileStruct[]?> HandleFileUpload(HttpListenerRequest request, HttpListenerResponse response, Stream stream)
        {
            // Extract boundary
            string? contentType = request.ContentType;
            if (contentType == null) return null;

            int boundaryIndex = contentType.IndexOf("boundary=") + 9;
            string boundary = contentType.Substring(boundaryIndex).Trim('"');

            try
            {
                return new FormDataUtil(stream, boundary, request.ContentEncoding).Files.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
            finally
            {
                stream.Close();
            }
        }
    }
}
