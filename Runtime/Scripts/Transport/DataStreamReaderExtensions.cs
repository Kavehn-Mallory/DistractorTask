using System;
using Unity.Collections;

namespace DistractorTask.Transport
{
    public static class DataStreamReaderExtensions
    {
        /// <summary>
        /// Extension method, allowing reading and writing of arbitrarily long strings
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ReadString(this ref DataStreamReader reader)
        {
            var fixedStringType = reader.ReadByte();

            switch (fixedStringType)
            {
                case 0: return reader.ReadFixedString32().ToString();
                case 1: return reader.ReadFixedString64().ToString();
                case 2: return reader.ReadFixedString128().ToString();
                case 3: return reader.ReadFixedString512().ToString();
                case 4: return reader.ReadFixedString4096().ToString();
                case 5: return reader.ReadFixedString4096() + reader.ReadString();
            }
            throw new ArgumentException($"The reader does not contain a fixed string that was sent by {nameof(DataStreamWriterExtensions.WriteString)}"); 
        }
        
    }
}