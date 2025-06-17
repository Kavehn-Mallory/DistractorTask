using System;
using DistractorTask.Transport.DataContainer.GenericClasses;
using DistractorTask.UserStudy.MarkerPointStage;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    [Serializable]
    public class MarkerCountData : BaseRespondingData<MarkerPointResponseData>
    {
        public int markerCount;


        public MarkerCountData()
        {
        }

        public override MarkerPointResponseData GenerateResponse() => new MarkerPointResponseData(SenderId, MessageId);

        public override void Serialize(ref DataStreamWriter writer)
        {
            base.Serialize(ref writer);
            writer.WriteInt(markerCount);
        }

        public override void Deserialize(ref DataStreamReader reader)
        {
            base.Deserialize(ref reader);
            markerCount = reader.ReadInt();
        }
    }
}