using System;
using System.Globalization;
using DistractorTask.Core;
using Unity.Collections;
using Unity.Networking.Transport;

namespace DistractorTask.Transport.DataContainer
{
    [Obsolete]
    public struct LogfileDataOld : ISerializer, ILogSerializer
    {

        public TimeSpan Time;
        public NetworkEndpoint NetworkEndpoint;
        public LogCategoryOld LogCategoryOld;
        public string Message;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteString(Time.ToString("c", CultureInfo.InvariantCulture));
            var ipAddressData = new IpAddressData
            {
                Endpoint = NetworkEndpoint
            };
            ipAddressData.Serialize(ref writer);
            writer.WriteString(LogCategoryOld.ToString());
            writer.WriteString(Message);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            Time = TimeSpan.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
            var ipAddressData = new IpAddressData();
            ipAddressData.Deserialize(ref reader);
            NetworkEndpoint = ipAddressData.Endpoint;
            LogCategoryOld = Enum.Parse<LogCategoryOld>(reader.ReadString());
            Message = reader.ReadString();
        }

        public string Serialize()
        {
            return $"{Time:c};{LogCategoryOld.ToString()};{NetworkEndpoint.ToString()};{LogCategoryOld.ToString()};{Message}";
        }

        public LogCategoryOld CategoryOld => LogCategoryOld.Logging;
    }
    
    [Obsolete]
    public enum LogCategoryOld
    {
        Default,
        Network,
        UserStudy,
        VideoPlayer,
        Logging
    }
}