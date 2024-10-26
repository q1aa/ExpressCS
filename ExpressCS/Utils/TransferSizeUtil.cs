using ExpressCS.Struct;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExpressCS.Utils
{
    internal class DownloadSizeUtil
    {
        public static void AddDownloadSize(NameValueCollection headers, long bodyContentLenght)
        {
            TransferSizeStorage.TotalDownloadSize += CalculateHeaderSize(headers) + bodyContentLenght;
        }

        public static void AddWebSocketDownloadSize(int dataSize)
        {
            TransferSizeStorage.TotalDownloadSize += dataSize;
        }

        public static int CalculateHeaderSize(NameValueCollection headers)
        {
            int returnSize = 0;
            foreach (var header in headers)
            {
                returnSize += Encoding.UTF8.GetByteCount(header.ToString());
            }

            return returnSize;
        }
    }

    internal class UploadSizeUtil
    {
        public static void AddUploadSize(NameValueCollection headers, long bodyContentLenght)
        {
            TransferSizeStorage.TotalUploadSize += CalculateHeaderSize(headers) + bodyContentLenght;
        }

        public static void AddWebSocketUploadSize(int dataSize)
        {
            TransferSizeStorage.TotalUploadSize += dataSize;
        }

        private static int CalculateHeaderSize(NameValueCollection headers)
        {
            int returnSize = 0;
            foreach (string header in headers)
            {
                returnSize += Encoding.UTF8.GetByteCount(header) + Encoding.UTF8.GetByteCount(headers[header]);
            }

            return returnSize;
        }
    }

    internal class SizeUpdaterUtil
    {
        public static Task StartTransferUpdateTimer(int interval = 200)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                LogUtil.LogWarning("The transfer size display is only available on Windows");
                return Task.CompletedTask;
            }
            
            Task.Run(async () =>
            {
                while (true)
                {
                    TransferedDataStruct transferData = GetTransferMessage();
                    ConsoleTitleUtil.ChangeTitle($"Downloaded: {transferData.DownloadSize} {transferData.DownloadUnit} | Uploaded: {transferData.UploadSize} {transferData.UploadUnit}");
                    await Task.Delay(interval);
                }
            });

            return Task.CompletedTask;
        }

        private static TransferedDataStruct GetTransferMessage()
        {
            TransferedUnit downloadUnit = TransferSizeStorage.TotalDownloadSize switch
            {
                < 1024 => TransferedUnit.B,
                < 1048576 => TransferedUnit.KB,
                _ => TransferedUnit.MB,
            };

            TransferedUnit uploadUnit = TransferSizeStorage.TotalUploadSize switch
            {
                < 1024 => TransferedUnit.B,
                < 1048576 => TransferedUnit.KB,
                _ => TransferedUnit.KB,
            };

            long downloadSize = downloadUnit switch
            {
                TransferedUnit.B => TransferSizeStorage.TotalDownloadSize,
                TransferedUnit.KB => TransferSizeStorage.TotalDownloadSize / 1024,
                _ => TransferSizeStorage.TotalDownloadSize / 1048576,
            };

            long uploadSize = uploadUnit switch
            {
                TransferedUnit.B => TransferSizeStorage.TotalUploadSize,
                TransferedUnit.KB => TransferSizeStorage.TotalUploadSize / 1024,
                _ => TransferSizeStorage.TotalUploadSize / 1048576,
            };

            return new TransferedDataStruct(uploadSize, uploadUnit, downloadSize, downloadUnit);
        }
    }
}
