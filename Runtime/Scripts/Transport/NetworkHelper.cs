using System.Net;
using System.Net.Sockets;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;

namespace DistractorTask.Transport
{
    public static class NetworkHelper
    {

        public const ushort IpListeningPort = 7500;
        public const ushort DefaultPort = 7777;
        
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            return null;
        }
        
        
        public static bool DidConnectionThrowError(ConnectionState reason)
        {
            return reason != ConnectionState.ClosedByRemote && reason != ConnectionState.Timeout;
        }
        
        public static ConnectionState CastDisconnectReasonToConnectionState(DisconnectReason reason)
        {
            switch (reason)
            {
                case DisconnectReason.Default : return ConnectionState.Default;
                case DisconnectReason.Timeout : return ConnectionState.Timeout;
                case DisconnectReason.MaxConnectionAttempts : return ConnectionState.MaxConnectionAttempts;
                case DisconnectReason.ClosedByRemote : return ConnectionState.ClosedByRemote;
                case DisconnectReason.AuthenticationFailure : return ConnectionState.AuthenticationFailure;
                case DisconnectReason.ProtocolError : return ConnectionState.ProtocolError;
            }

            return ConnectionState.Connected;
        }

        public static NetworkEndpoint GetLocalIpListeningEndpoint()
        {
            return GetLocalEndpoint(IpListeningPort);
        }
        
        public static NetworkEndpoint GetLocalEndpointWithDefaultPort()
        {
            return GetLocalEndpoint(DefaultPort);
        }
        
        public static NetworkEndpoint GetLocalEndpoint(ushort port)
        {
            var ip = NetworkHelper.GetLocalIPAddress();
            var endpoint = NetworkEndpoint.Parse(ip.ToString(), port);
            //if we are listening for ip in the editor we are doing it just locally?
#if UNITY_EDITOR
            endpoint = NetworkEndpoint.LoopbackIpv4.WithPort(port);
#endif

            return endpoint;
        }
    }
}