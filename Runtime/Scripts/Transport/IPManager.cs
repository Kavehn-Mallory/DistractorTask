using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DistractorTask.Transport
{
    public class IPManager
    {
        public static IPAddress GetIP(ADDRESSFAM addressFamily)
        {
            //Return null if ADDRESSFAM is Ipv6 but Os does not support it
            if (addressFamily == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
            {
                return null;
            }


            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        //IPv4
                        if (addressFamily == ADDRESSFAM.IPv4)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address;
                            }
                        }

                        //IPv6
                        else if (addressFamily == ADDRESSFAM.IPv6)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                return ip.Address;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }

    public enum ADDRESSFAM
    {
        IPv4, IPv6
    }
}