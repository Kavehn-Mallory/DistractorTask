using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer.GenericClasses
{
    public abstract class BaseResponseData : IResponseIdentifier, ISerializer
    {
        public int SenderId { get; set; }
        public int MessageId { get; set; }
        public virtual void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteInt(SenderId);
            writer.WriteInt(MessageId);
        }

        public virtual void Deserialize(ref DataStreamReader reader)
        {
            SenderId = reader.ReadInt();
            MessageId = reader.ReadInt();
        }
    }
    
}