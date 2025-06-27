using DistractorTask.Transport.DataContainer.GenericClasses;
using Unity.Collections;
using UnityEngine;

namespace DistractorTask.Logging.Components
{
    public abstract class Vector3Data : GenericOneValueData<Vector3>
    {
        public override void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteFloat(Value.x);
            writer.WriteFloat(Value.y);
            writer.WriteFloat(Value.z);
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            var x = reader.ReadFloat();
            var y = reader.ReadFloat();
            var z = reader.ReadFloat();
            Value = new Vector3(x, y, z);
        }
    }
    
    public class HeadPositionData : Vector3Data
    {
        
    }

    public class EyetrackingLookDirection : Vector3Data
    {
        
    }
    
    
}