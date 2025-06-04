using System;
using DistractorTask.Core;
using DistractorTask.Transport.DataContainer.GenericClasses;
using Unity.Collections;

namespace DistractorTask.UserStudy.DataDrivenSetup
{
    [Serializable]
    public class MarkerPointCountData : BaseRespondingData<MarkerPointCountReceived>
    {
        public int markerCount;

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

    public class MarkerPointCountReceived : GenericNoValueData, IResponseIdentifier
    {
        public int SenderId { get; set; }
        public int MessageId { get; set; }
    }
}