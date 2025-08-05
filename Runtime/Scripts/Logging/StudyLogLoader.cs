using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DistractorTask.UserStudy.Core;
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

    public class StudyLogRuntime
    {
        private TimeSpan[] _timeStamps;
        private LogData[] _logData;
        private EventTimings[] _eventTimings;
        private StudyData[] _studyData;
        private TrialData[] _trials;

        public StudyLogRuntime(StudyLogLoader.LogEvent[] logData)
        {
            GenerateStudyTimings(logData);
        }
        
        private void GenerateStudyTimings(StudyLogLoader.LogEvent[] logEvents)
        {
            var studyTimings = new List<EventTimings>();
            var studyData = new List<StudyData>();
            var studyBegin = 0;
            var trialCount = 0;
            var actualTrialCount = 0;
            var repetitionsPerTrial = 0;
            var trialBegin = 0;
            var trials = new List<TrialData>();
            var trialIndices = new List<int>();

            _logData = new LogData[logEvents.Length];
            _timeStamps = new TimeSpan[logEvents.Length];
            
            for (int i = 0; i < logEvents.Length; i++)
            {
                var logEvent = logEvents[i];
                _logData[i] = logEvent.logData;
                _timeStamps[i] = logEvent.timeStamp;
                
                if (logEvent.logData.LogCategory == LogCategory.StudyBegin)
                {
                    if (trialBegin > 0)
                    {
                        Debug.LogWarning($"Study {logEvents[studyBegin].logData.StudyName1} has a trial without TrialEnd entry");
                        CreateTrialEventTiming(ref studyTimings, ref trialBegin, i, ref trials, ref trialIndices);
                    }
                    if (trialCount > 0)
                    {
                        Debug.LogWarning($"Study {logEvents[studyBegin].logData.StudyName1} has no StudyEnd entry");
                        CreateStudyEventTiming(ref studyTimings, ref studyBegin, i - 1, ref studyData, ref trialCount, ref actualTrialCount, ref repetitionsPerTrial);
                    }
                    studyBegin = i;
                }
                if (logEvent.logData.LogCategory == LogCategory.StudyEnd)
                {
                    CreateStudyEventTiming(ref studyTimings, ref studyBegin, i, ref studyData, ref trialCount, ref actualTrialCount, ref repetitionsPerTrial);
                }

                if (logEvent.logData.LogCategory == LogCategory.TrialBegin)
                {
                    if (trialBegin > 0)
                    {
                        Debug.LogWarning($"Study {logEvents[studyBegin].logData.StudyName1} has a trial without TrialEnd entry");
                        CreateTrialEventTiming(ref studyTimings, ref trialBegin, i, ref trials, ref trialIndices);
                    }
                    trialCount = logEvent.logData.TrialCount1;
                    repetitionsPerTrial = logEvent.logData.RepetitionsPerTrial1;
                    trialBegin = i;
                }

                if (logEvent.logData.LogCategory == LogCategory.TrialConfirmation)
                {
                    actualTrialCount++;
                    trialIndices.Add(i);
                }

                if (logEvent.logData.LogCategory == LogCategory.TrialEnd)
                {
                    CreateTrialEventTiming(ref studyTimings, ref trialBegin, i, ref trials, ref trialIndices);
                }
            }

            _eventTimings = studyTimings.ToArray();
            _studyData = studyData.ToArray();
            _trials = trials.ToArray();
        }

        private static void CreateStudyEventTiming(ref List<EventTimings> studyTimings, ref int begin, int currentIndex, ref List<StudyData> studyData, ref int trialCount,
            ref int actualTrialCount, ref int repetitionsPerTrial)
        {
            studyTimings.Add(new EventTimings
            {
                StartIndex = begin,
                EndIndex = currentIndex,
                EventType = StudyEventType.StudyStage,
                DataIndex = studyData.Count
            });
            studyData.Add(new StudyData
            {
                TrialCount = trialCount,
                ActualTrialCount = actualTrialCount / repetitionsPerTrial,
                RepetitionsPerTrial = repetitionsPerTrial
                        
            });

            trialCount = 0;
            actualTrialCount = 0;
            repetitionsPerTrial = 0;
        }
        
        private static void CreateTrialEventTiming(ref List<EventTimings> studyTimings, ref int begin, int currentIndex, ref List<TrialData> trialData, ref List<int> trials)
        {
            studyTimings.Add(new EventTimings
            {
                StartIndex = begin,
                EndIndex = currentIndex,
                EventType = StudyEventType.Trial,
                DataIndex = trialData.Count
            });
            trialData.Add(new TrialData()
            {
                Trials = trials.ToArray()
            });
            begin = 0;
            trials.Clear();
        }

        private string GetStudyStageName(int index)
        {
            return _logData[index].StudyName1;
        }
        
        private string GetParticipantType(int index)
        {
            return _logData[index].ParticipantType1;
        }

        private TrialTiming[] CalculateTrialTimings()
        {
            var trialTimings = new List<TrialTiming>();
            foreach (var eventTiming in _eventTimings)
            {
                if (eventTiming.EventType == StudyEventType.Trial)
                {
                    trialTimings.Add(new TrialTiming
                    {
                        LoadLevel = _logData[eventTiming.StartIndex].LoadLevel1,
                        NoiseLevel = _logData[eventTiming.StartIndex].NoiseLevel1
                    });
                    var trials = _trials[eventTiming.DataIndex];

                    var timings = new TimeSpan[trials.Trials.Length];

                    for (var i = 0; i < trials.Trials.Length; i++)
                    {
                        var trial = trials.Trials[i];
                        var trialData = _logData[trial];

                        var duration =
                            new TimeSpan(trialData.ReactionTime1).Subtract(new TimeSpan(trialData.StartTime1));

                        timings[i] = duration;
                    }

                    var result = trialTimings[^1];
                    result.Durations = timings;
                    trialTimings[^1] = result;
                }
            }

            return trialTimings.ToArray();
        }
    }

    public struct StudyData
    {
        public int TrialCount;
        public int ActualTrialCount;
        public int RepetitionsPerTrial;

    }

    public struct TrialTiming
    {
        public NoiseLevel NoiseLevel;
        public LoadLevel LoadLevel;
        public TimeSpan[] Durations;
    }

    public struct TrialData
    {
        public int[] Trials;
    }

    public struct EventTimings
    {
        public int StartIndex;
        public int EndIndex;
        public StudyEventType EventType;
        public int DataIndex;
    }

    public enum StudyEventType
    {
        StudyStage,
        Trial
    }
}