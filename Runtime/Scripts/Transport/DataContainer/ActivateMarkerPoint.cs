using System;
using DistractorTask.Core;
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
    
    public class OnMarkerPointActivatedData : BaseRespondingData<OnAnchorPointSelectionData>
    {
        public int MarkerPointIndex = -1;
        
        public override void Serialize(ref DataStreamWriter writer)
        {
            base.Serialize(ref writer);
            writer.WriteInt(MarkerPointIndex);
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            base.Deserialize(ref reader);
            MarkerPointIndex = reader.ReadInt();
        }
        
    }

    public class OnAnchorPointSelectionData : BaseResponseData
    {

    }
}