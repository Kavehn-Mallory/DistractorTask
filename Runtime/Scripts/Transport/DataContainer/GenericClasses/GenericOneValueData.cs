using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer.GenericClasses
{
    public abstract class GenericOneValueData<T> : ISerializer
    {

        public T Value;
        
        public abstract void Serialize(ref DataStreamWriter writer);

        public abstract void Deserialize(ref DataStreamReader reader);
    }
}