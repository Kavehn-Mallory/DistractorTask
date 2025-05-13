using DistractorTask.Transport.DataContainer.GenericClasses;
using Unity.Collections;
using Unity.Networking.Transport;

namespace DistractorTask.Transport
{
    public class IpAddressDataV2 : BaseRespondingData<IpAddressResponseData>
    {
        public NetworkEndpoint Endpoint;
        
        
        public override void Serialize(ref DataStreamWriter writer)
        {
            base.Serialize(ref writer);
            var address = Endpoint.ToFixedString();
            writer.WriteFixedString128(address);
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            base.Deserialize(ref reader);
            var address = reader.ReadFixedString128().ToString().Split(':');
            var ip = address[0];
            var port = ushort.Parse(address[1]);

            Endpoint = NetworkEndpoint.Parse(ip, port);
        }
        
    }
    
    public class IpAddressResponseData : BaseResponseData
    {
    }
}