using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer.GenericClasses
{
    public abstract class GenericKeyValueData<T, TS> : ISerializer
    {
        public T Key;
        public TS Value;
        
        public abstract void Serialize(ref DataStreamWriter writer);

        public abstract void Deserialize(ref DataStreamReader reader);
    }
    

}