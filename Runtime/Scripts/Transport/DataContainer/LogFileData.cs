using DistractorTask.Core;
using DistractorTask.Logging;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    public class LogFileData : ISerializer
    {

        public string Value;

        public LogFileData()
        {
            Value = LogData.WriteLogData(new LogData());
        }
        
        public LogFileData(LogData logData)
        {
            Value = LogData.WriteLogData(logData);
        }
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteString(Value);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            Value = reader.ReadString();
        }
    }
}