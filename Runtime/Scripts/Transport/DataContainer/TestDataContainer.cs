using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    public class TestDataContainer : ISerializer
    {
        public void Serialize(ref DataStreamWriter writer)
        {
            
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            
        }
    }

    public abstract class BaseDataContainer : ISerializer
    {
        public abstract void Serialize(ref DataStreamWriter writer);

        public abstract void Deserialize(ref DataStreamReader reader);
    }

    public class DerivedDataContainer : BaseDataContainer
    {
        public override void Serialize(ref DataStreamWriter writer)
        {
            
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            
        }
    }
}