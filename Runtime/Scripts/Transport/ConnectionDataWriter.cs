using System;
using DistractorTask.Core;
using Unity.Collections;

namespace DistractorTask.Transport
{
    public struct ConnectionDataWriter
    {

        private const int ReservedByteCount = 4;
        
        public static void SendMessage(ref DataStreamWriter writer, in ISerializer serializableData)
        {
            var index = GetDataTypeIndex(serializableData.GetType());
            writer.WriteByte(index);
            serializableData.Serialize(ref writer);
        }

        private static byte GetDataTypeIndex(Type type)
        {
            return DataSerializationIndexer.GetTypeIndex(type);
        }

        public static void WriteString(ref DataStreamWriter writer, string data)
        {
            while (true)
            {
                //I believe that 3 bytes are reserved for the fixed string itself. We also need one byte to determine the length of the fixed string
                //the extra byte might not be needed, but I'd rather be safe than sorry

                var length = data.Length * sizeof(char) + ReservedByteCount;

                switch (length)
                {
                    case <= 32:
                        writer.WriteByte(0);
                        writer.WriteFixedString32(data);
                        return;
                    case <= 64:
                        writer.WriteByte(1);
                        writer.WriteFixedString64(data);
                        return;
                    case <= 128:
                        writer.WriteByte(2);
                        writer.WriteFixedString128(data);
                        return;
                    case <= 512:
                        writer.WriteByte(3);
                        writer.WriteFixedString512(data);
                        return;
                    case <= 4096:
                        writer.WriteByte(4);
                        writer.WriteFixedString4096(data);
                        return;
                }

                //We need to split the string into smaller parts 
                writer.WriteByte(5);
                writer.WriteFixedString4096(data[..(4096 - ReservedByteCount)]);
                data = data[(4096 - ReservedByteCount)..];

                //throw new ArgumentException("string is too long for all types of fixed string.");
            }
        }


        public static bool TryWriteString(ref DataStreamWriter writer, string data)
        {
            try
            {
                WriteString(ref writer, data);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        
    }
}