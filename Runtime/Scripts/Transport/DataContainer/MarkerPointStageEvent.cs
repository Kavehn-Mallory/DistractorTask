using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    public struct MarkerPointStageEvent : IStudyStageEvent
    {
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteByte(IsStartEvent ? (byte)1 : (byte)0);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            var readByte = reader.ReadByte();
            IsStartEvent = readByte == 1;
        }

        public bool IsStartEvent { get; set; }
    }
}