using System.Net;
using System.Net.Sockets;

namespace LuckyOffice.Utility
{
    public static class SocketHelper
    {
        public static int GetAvailablePort(int startingPort)
        {
            int port = startingPort;
            while (!IsPortAvailable(port))
            {
                port++;
            }
            return port;
        }

        public static bool IsPortAvailable(int port)
        {
            try
            {
                var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch (System.Net.Sockets.SocketException)
            {
                return false;
            }
        }
    }
}
