using System;
using DistractorTask.Core;
using DistractorTask.Transport.DataContainer;
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

        public override string Serialize()
        {
            return base.Serialize() + $"{nameof(markerCount)}: {markerCount.ToString()}";
        }

        public override LogCategory Category => LogCategory.UserStudy;
    }

    public class MarkerPointCountReceived : GenericNoValueData, IResponseIdentifier
    {
        public int SenderId { get; set; }
        public int MessageId { get; set; }
    }
}