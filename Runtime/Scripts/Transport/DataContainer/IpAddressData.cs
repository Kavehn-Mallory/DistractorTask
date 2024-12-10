using DistractorTask.Core;
using Unity.Collections;
using Unity.Networking.Transport;

namespace DistractorTask.Transport.DataContainer
{
    public struct IpAddressData : ISerializer
    {

        public NetworkEndpoint Endpoint;
        
        
        public void Serialize(ref DataStreamWriter writer)
        {
            var address = Endpoint.ToFixedString();
            writer.WriteFixedString128(address);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            var address = reader.ReadFixedString128().ToString().Split(':');
            var ip = address[0];
            var port = ushort.Parse(address[1]);

            Endpoint = NetworkEndpoint.Parse(ip, port);
        }
    }
}