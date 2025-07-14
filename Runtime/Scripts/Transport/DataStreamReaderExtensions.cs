using System;
using System.Globalization;
using DistractorTask.UserStudy;
using DistractorTask.UserStudy.Core;
using Unity.Collections;
using UnityEngine;

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

        public static bool ReadBoolean(this ref DataStreamReader reader)
        {
            return reader.ReadByte() == 1;
        }

        public static StudyCondition ReadStudyCondition(this ref DataStreamReader reader)
        {

            var loadLevel = Enum.Parse<LoadLevel>(reader.ReadByte().ToString());
            var noiseLevel = Enum.Parse<NoiseLevel>(reader.ReadByte().ToString());
            var repetitionsPerTrial = reader.ReadInt();
            var trialCount = reader.ReadInt();
            var hasAudioTask = reader.ReadBoolean();
            var insideWall = reader.ReadBoolean();
            
            return new StudyCondition(loadLevel,
                noiseLevel, trialCount, repetitionsPerTrial, hasAudioTask, insideWall);
        }


    }
}