using System;
using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    [Serializable]
    public struct SceneGroupChangeData : ISerializer
    {

        public int index;
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteInt(index);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            index = reader.ReadInt();
        }
    }
}