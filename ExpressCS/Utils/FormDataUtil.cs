using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExpressCS.Struct;

namespace TestHttpListenerContext
{
    public class FormDataUtil
    {
        public List<ReceiveFileStruct> Files { get; } = new List<ReceiveFileStruct>();
        public Dictionary<string, string> Fields { get; } = new Dictionary<string, string>();

        public FormDataUtil(Stream stream, string boundary, Encoding encoding)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrEmpty(boundary))
                throw new ArgumentNullException(nameof(boundary));

            encoding = encoding ?? Encoding.UTF8;
            ParseMultipartContent(stream, boundary, encoding).Wait();
        }

        private async Task ParseMultipartContent(Stream stream, string boundary, Encoding encoding)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                ms.Position = 0;

                MultipartFormDataContent content = new MultipartFormDataContent("--" + boundary);
                content.Headers.Remove("Content-Type");
                content.Headers.TryAddWithoutValidation("Content-Type", $"multipart/form-data; boundary=\"{boundary}\"");

                // Create a temporary HttpContent to extract the parts
                using (StreamContent tempContent = new StreamContent(ms))
                {
                    tempContent.Headers.Remove("Content-Type");
                    MultipartReader reader = new MultipartReader(boundary, ms);
                    MultipartSection section;
                    while ((section = await reader.ReadNextSectionAsync()) != null)
                    {
                        ContentDispositionHeaderValue  contentDisposition = section.GetContentDispositionHeader();
                        if (contentDisposition.IsFileDisposition())
                        {
                            FileMultipartSection fileSection = section.AsFileSection();
                            MemoryStream memoryStream = new MemoryStream();
                            await fileSection.FileStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;

                            Files.Add(new ReceiveFileStruct
                            {
                                Name = fileSection.Name,
                                FileName = fileSection.FileName,
                                ContentType = section.ContentType ?? "application/octet-stream",
                                Data = memoryStream
                            });
                        }
                        else if (contentDisposition.IsFormDisposition())
                        {
                            FormMultipartSection formSection = section.AsFormDataSection();
                            var value = await formSection.GetValueAsync();
                            Fields[formSection.Name] = value;
                        }
                    }
                }
            }
        }
    }
}