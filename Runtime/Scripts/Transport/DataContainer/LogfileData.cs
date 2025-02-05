using System;
using System.Globalization;
using DistractorTask.Core;
using Unity.Collections;
using Unity.Networking.Transport;

namespace DistractorTask.Transport.DataContainer
{
    public struct LogfileData : ISerializer
    {

        public TimeSpan Time;
        public NetworkEndpoint NetworkEndpoint;
        public LogCategory LogCategory;
        public string Message;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteString(Time.ToString("c", CultureInfo.InvariantCulture));
            var ipAddressData = new IpAddressData
            {
                Endpoint = NetworkEndpoint
            };
            ipAddressData.Serialize(ref writer);
            writer.WriteString(LogCategory.ToString());
            writer.WriteString(Message);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            Time = TimeSpan.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
            var ipAddressData = new IpAddressData();
            ipAddressData.Deserialize(ref reader);
            NetworkEndpoint = ipAddressData.Endpoint;
            LogCategory = Enum.Parse<LogCategory>(reader.ReadString());
            Message = reader.ReadString();
        }
    }

    public enum LogCategory
    {
        Default,
        Network,
        UserStudy
    }
}