using System;
using System.IO;
using UnityEngine;

namespace DistractorTask.Logging
{
    public class StudyLogLoader : MonoBehaviour
    {

        //todo change this to a folder and just load all the files. I want an editor window where we can choose logfiles based on date or userId 

        [SerializeField]
        private string logFilePath;

        private LogEvent[] logEvents;

        // Use this for initialization
        void Start()
        {

        }

        private static LogEvent[] LoadLogFile(string path)
        {
            var text = File.ReadAllLines(path);
            var result = new LogEvent[text.Length - 1];

            for (var i = 1; i < text.Length; i++)
            {
                result[i - 1] = new LogEvent
                {
                    timeStamp = ReadTimeStamp(text[i].Split(';')[0]),
                    logData = LogData.LoadLogDataFromCSVLine(text[0], text[i])
                };
            }


            return result;
        }

        private static TimeSpan ReadTimeStamp(string v)
        {
            return TimeSpan.Parse(v);
        }

        [Serializable]
        public struct LogEvent
        {
            public TimeSpan timeStamp;
            public LogData logData;
        }
    }
}