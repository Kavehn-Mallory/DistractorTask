using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer.GenericClasses
{
    public abstract class GenericNoValueData : ISerializer
    {
        public void Serialize(ref DataStreamWriter writer)
        {
            
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            
        }
    }
}