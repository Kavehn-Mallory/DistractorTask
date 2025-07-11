﻿using System.Net;
using System.Net.NetworkInformation;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;

namespace DistractorTask.Transport
{
    public static class NetworkExtensions
    {

        public const ushort IpListeningPort = 7500;
        public const ushort DefaultPort = 7777;
        public const ushort DisplayWallControlPort = 7600;
        public const ushort LoggingPort = 7400;
        
        public static IPAddress GetLocalIPAddress()
        {

            foreach(NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if(ni.NetworkInterfaceType is NetworkInterfaceType.Wireless80211 or NetworkInterfaceType.Ethernet)
                {

                    if (ni.GetIPProperties().GatewayAddresses.Count == 0)
                    {
                        continue;
                    }
                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {

                            return ip.Address;
                        }
                    }
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
            return GetLocalEndpoint(IpListeningPort, true);
        }
        
        public static NetworkEndpoint GetLocalEndpointWithDefaultPort(bool binding)
        {
            return GetLocalEndpoint(DefaultPort, binding);
        }
        
        public static NetworkEndpoint GetBindableEndpoint(ushort port)
        {
            return NetworkEndpoint.AnyIpv4.WithPort(port);
        }
        
        
        public static NetworkEndpoint GetLocalEndpoint(ushort port, bool binding)
        {
            var ip = GetLocalIPAddress();
            var endpoint = NetworkEndpoint.Parse(ip.ToString(), port);
            //endpoint = NetworkEndpoint.Parse("192.168.1.110", port);
            if(binding)
                endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);

            return endpoint;
        }

        public static bool IsLocalAddress(this NetworkEndpoint endpoint)
        {
            return GetLocalEndpoint(endpoint.Port, true) == endpoint ||
                   GetLocalEndpoint(endpoint.Port, false) == endpoint || 
                   endpoint.IsLoopback || endpoint == NetworkEndpoint.AnyIpv4.WithPort(endpoint.Port);
        }
        
        
    }
}