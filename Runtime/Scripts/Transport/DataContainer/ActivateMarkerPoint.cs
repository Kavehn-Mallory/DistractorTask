using System;
using DistractorTask.Transport.DataContainer.GenericClasses;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    [Serializable]
    public class ActivateMarkerPoint : BaseRespondingData<OnMarkerPointActivatedData>
    {

        public ActivateMarkerPoint()
        {
            
        }
        public ActivateMarkerPoint(int markerIndex)
        {
            currentMarkerIndex = markerIndex;
        }
        public int currentMarkerIndex;
        public override void Serialize(ref DataStreamWriter writer)
        {
            base.Serialize(ref writer);
            writer.WriteInt(currentMarkerIndex);
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            base.Deserialize(ref reader);
            currentMarkerIndex = reader.ReadInt();
        }
    }
    
    public class OnMarkerPointActivatedData : BaseResponseData
    {
        
    }
}