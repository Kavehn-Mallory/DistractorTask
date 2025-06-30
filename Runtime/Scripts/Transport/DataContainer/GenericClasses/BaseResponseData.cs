using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer.GenericClasses
{
    public abstract class BaseResponseData : IResponseIdentifier, ISerializer, ILogSerializer
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

        public virtual string Serialize()
        {
            return $"{nameof(SenderId)}: {SenderId.ToString()};{nameof(MessageId)};{MessageId.ToString()}";
        }

        public virtual LogCategoryOld CategoryOld => LogCategoryOld.UserStudy;
    }
    
}