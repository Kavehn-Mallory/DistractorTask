using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    public struct TrialCompletedData : ISerializer
    {

        public byte LoadLevel;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte(LoadLevel);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            LoadLevel = reader.ReadByte();
        }
    }
}