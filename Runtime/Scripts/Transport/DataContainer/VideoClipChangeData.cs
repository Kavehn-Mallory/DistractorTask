using System;
using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    [Serializable]
    public struct VideoClipChangeData : ISerializer
    {

        public int videoClipIndex;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteInt(videoClipIndex);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            videoClipIndex = reader.ReadInt();
        }
    }
}