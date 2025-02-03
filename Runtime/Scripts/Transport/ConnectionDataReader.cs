using System;
using Unity.Collections;

namespace DistractorTask.Transport
{
    public struct ConnectionDataReader
    {
        public static string ReadFixedString(ref DataStreamReader reader)
        {
            var fixedStringType = reader.ReadByte();

            switch (fixedStringType)
            {
                case 0: return reader.ReadFixedString32().ToString();
                case 1: return reader.ReadFixedString64().ToString();
                case 2: return reader.ReadFixedString128().ToString();
                case 3: return reader.ReadFixedString512().ToString();
                case 4: return reader.ReadFixedString4096().ToString();
                case 5: return reader.ReadFixedString4096() + ReadFixedString(ref reader);
            }
            throw new ArgumentException($"The reader does not contain a fixed string that was sent by {nameof(ConnectionDataWriter.WriteString)}"); 
        }

        public static bool TryReadFixedString(ref DataStreamReader reader, out string data)
        {
            try
            {
                data = ReadFixedString(ref reader);
                return true;
            }
            catch
            {
                data = "";
                return false;
            }
        }
    }
}