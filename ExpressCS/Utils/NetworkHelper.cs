using System.Net.NetworkInformation;

namespace ExpressCS.Utils
{
    internal class NetworkHelper
    {
        public static bool IsPortInUse(int port)
        {
            return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(endPoint => endPoint.Port == port);
        }
    }
}
