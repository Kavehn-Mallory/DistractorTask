using System;
using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    [Serializable]
    public struct MarkerCountData : ISerializer
    {
        //todo maybe replace this with just an int?
        public int markerCount;
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteInt(markerCount);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            markerCount = reader.ReadInt();
        }
    }
}