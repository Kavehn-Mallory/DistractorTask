using DistractorTask.Transport.DataContainer.GenericClasses;
using Unity.Collections;

namespace DistractorTask.Logging.Components
{
    public abstract class DistanceData : GenericOneValueData<float>
    {
        public override void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteFloat(Value);
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            Value = reader.ReadFloat();
        }
    }
    
    public class DistanceToDistractorData : DistanceData
    {
    
    }
}