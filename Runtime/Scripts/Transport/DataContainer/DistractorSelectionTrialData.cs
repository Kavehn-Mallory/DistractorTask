using System;
using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    [Serializable]
    public struct DistractorSelectionTrialData : ISerializer
    {
        public int[] markers;
        public byte loadLevel;
        public int selectionCount;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            
            writer.WriteByte(loadLevel);
            writer.WriteInt(selectionCount);
            writer.WriteInt(markers.Length);
            for (int i = 0; i < markers.Length; i++)
            {
                writer.WriteInt(markers[i]);
            }
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            
            loadLevel = reader.ReadByte();
            selectionCount = reader.ReadInt();
            var markerCount = reader.ReadInt();
            markers = new int[markerCount];

            for (int i = 0; i < markers.Length; i++)
            {
                markers[i] = reader.ReadInt();
            }
        }
    }
}