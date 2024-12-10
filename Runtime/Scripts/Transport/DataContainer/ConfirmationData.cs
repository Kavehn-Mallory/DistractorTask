using System;
using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport.DataContainer
{
    [Serializable]
    public struct ConfirmationData : ISerializer
    {

        public int confirmationNumber;
        
        public void Serialize(ref DataStreamWriter writer)
        {
            writer.WriteInt(confirmationNumber);
        }

        public void Deserialize(ref DataStreamReader reader)
        {
            confirmationNumber = reader.ReadInt();
        }
    }
}