using DistractorTask.Transport.DataContainer;
using Unity.Collections;

namespace DistractorTask.Core
{
    public interface ISerializer
    {
        public void Serialize(ref DataStreamWriter writer);
        
        public void Deserialize(ref DataStreamReader reader);
    }

    public interface ILogSerializer
    {
        public string Serialize();
        
        public LogCategoryOld CategoryOld { get; }
    }
}