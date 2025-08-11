using System;
using System.Collections.Generic;
using System.Linq;
using DistractorTask.Logging;
using DistractorTask.UserStudy.Core;
using Unity.Mathematics;
using UnityEngine;

namespace DistractorTask.Editor.UI
{
    public class UserStudyEvaluation
    {
        //for each log file -> reaction time for each trial 

        private string[] _userIds = Array.Empty<string>();

        private StudyLogRuntime[] _studyLogs = Array.Empty<StudyLogRuntime>();

        public UserStudyEvaluation(RuntimeStudyData studyData)
        {
            _studyLogs = new StudyLogRuntime[studyData.logFiles.Count];
            _userIds = studyData.userIds;
            for (var i = 0; i < studyData.logFiles.Count; i++)
            {
                var logFile = studyData.logFiles[i];
                _studyLogs[i] = new StudyLogRuntime(logFile.logEvents);
            }
        }

        public ReactionTimeData[] CalculateReactionTimeForCondition(LoadLevel loadLevel, NoiseLevel noiseLevel)
        {
            var timings = new List<ReactionTimeData>();
            foreach (var studyLog in _studyLogs)
            {
                timings.AddRange(studyLog.CalculateTrialTimingsForCondition(loadLevel, noiseLevel).ReactionTimes);
            }
            return timings.ToArray();
        }

        public void Count()
        {
            Debug.Log($"Study count: {_studyLogs.Length}");
            var c = 0;
            foreach (var studyLog in _studyLogs)
            {
                c += studyLog.CountTrialCount();
            }
            Debug.Log(c);
        }

        public class StudyLogRuntime
        {
            private TimeSpan[] _timeStamps;
            private EventTimings[] _eventTimings;
            private StudyData[] _studyData;
            private TrialData[] _trials;
            private LogData[] _logData;

            public StudyLogRuntime(UserStudyEvaluationEditor.LogEvent[] logData)
            {
                GenerateStudyTimings(logData);
            }

            public int CountTrialCount()
            {
                var counter = 0;
                Debug.Log($"Data count: {_eventTimings.Length}");
                foreach (var evenTiming in _eventTimings)
                {
                    if (evenTiming.EventType == StudyEventType.Trial)
                    {
                        counter++;
                    }
                }

                return counter;
            }

            private void GenerateStudyTimings(UserStudyEvaluationEditor.LogEvent[] logEvents)
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
                    _timeStamps[i] = logEvent.timeStamp;
                    _logData[i] = logEvent.logData;

                    if (logEvent.logData.LogCategory == LogCategory.StudyBegin)
                    {
                        if (trialBegin > 0)
                        {
                            Debug.LogWarning(
                                $"Study {logEvents[studyBegin].logData.StudyName1} has a trial without TrialEnd entry");
                            CreateTrialEventTiming(ref studyTimings, ref trialBegin, i, ref trials, ref trialIndices);
                        }

                        if (trialCount > 0)
                        {
                            Debug.LogWarning($"Study {logEvents[studyBegin].logData.StudyName1} has no StudyEnd entry");
                            CreateStudyEventTiming(ref studyTimings, ref studyBegin, i - 1, ref studyData,
                                ref trialCount, ref actualTrialCount, ref repetitionsPerTrial);
                        }

                        studyBegin = i;
                    }

                    if (logEvent.logData.LogCategory == LogCategory.StudyEnd)
                    {
                        CreateStudyEventTiming(ref studyTimings, ref studyBegin, i, ref studyData, ref trialCount,
                            ref actualTrialCount, ref repetitionsPerTrial);
                    }

                    if (logEvent.logData.LogCategory == LogCategory.TrialBegin)
                    {
                        if (trialBegin > 0)
                        {
                            Debug.LogWarning(
                                $"Study {logEvents[studyBegin].logData.StudyName1} has a trial without TrialEnd entry");
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
                
                //todo make sure that there is no open study / trial etc. 

                _eventTimings = studyTimings.ToArray();
                _studyData = studyData.ToArray();
                _trials = trials.ToArray();
            }

            private static void CreateStudyEventTiming(ref List<EventTimings> studyTimings, ref int begin,
                int currentIndex, ref List<StudyData> studyData, ref int trialCount,
                ref int actualTrialCount, ref int repetitionsPerTrial)
            {
                repetitionsPerTrial = math.max(1, repetitionsPerTrial);
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
                    RepetitionsPerTrial = repetitionsPerTrial,
                    EventTimingIndex = studyTimings.Count - 1
                });

                trialCount = 0;
                actualTrialCount = 0;
                repetitionsPerTrial = 0;
            }

            private static void CreateTrialEventTiming(ref List<EventTimings> studyTimings, ref int begin,
                int currentIndex, ref List<TrialData> trialData, ref List<int> trials)
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
            
            
            public TrialTiming CalculateTrialTimingsForCondition(LoadLevel loadLevel, NoiseLevel noiseLevel)
            {
               
                var trialTimings = new TrialTiming
                {
                    LoadLevel = loadLevel,
                    NoiseLevel = noiseLevel
                    
                };
                var timings = new List<ReactionTimeData>();
                var counter = 0;
                foreach (var eventTiming in _eventTimings)
                {
                    if (eventTiming.EventType == StudyEventType.Trial && _logData[eventTiming.StartIndex].LoadLevel1 == loadLevel && _logData[eventTiming.StartIndex].NoiseLevel1 == noiseLevel)
                    {
                        counter++;
                        var trials = _trials[eventTiming.DataIndex];

                        for (var i = 0; i < trials.Trials.Length; i++)
                        {
                            var trial = trials.Trials[i];
                            var trialData = _logData[trial];

                            var duration =
                                new TimeSpan(trialData.ReactionTime1).Subtract(new TimeSpan(trialData.StartTime1));
                            
                            timings.Add(new ReactionTimeData
                            {
                                Duration = duration.Milliseconds,
                                Time = new TimeSpan(trialData.StartTime1).Milliseconds
                            });
                        }
                    }
                }

                Debug.Log($"{counter} trials found");
                trialTimings.ReactionTimes = timings.ToArray();

                return trialTimings;
            }

            public TrialTiming[] CalculateTrialTimingsForStudy(int studyIndex)
            {
                var study = _studyData[studyIndex];

                var eventTimings = _eventTimings[study.EventTimingIndex];
                
                var trialTimings = new List<TrialTiming>();
                for (var index = eventTimings.StartIndex; index <= eventTimings.EndIndex; index++)
                {
                    var eventTiming = _eventTimings[index];
                    if (eventTiming.EventType == StudyEventType.Trial)
                    {
                        trialTimings.Add(new TrialTiming
                        {
                            LoadLevel = _logData[eventTiming.StartIndex].LoadLevel1,
                            NoiseLevel = _logData[eventTiming.StartIndex].NoiseLevel1
                        });
                        var trials = _trials[eventTiming.DataIndex];

                        var timings = new ReactionTimeData[trials.Trials.Length];

                        for (var i = 0; i < trials.Trials.Length; i++)
                        {
                            var trial = trials.Trials[i];
                            var trialData = _logData[trial];

                            var duration =
                                new TimeSpan(trialData.ReactionTime1).Subtract(new TimeSpan(trialData.StartTime1)).Milliseconds;

                            timings[i] = new ReactionTimeData
                            {
                                Duration = duration,
                                Time = new TimeSpan(trialData.StartTime1).Milliseconds
                            };
                        }

                        var result = trialTimings[^1];
                        result.ReactionTimes = timings;
                        trialTimings[^1] = result;
                    }
                }

                return trialTimings.ToArray();
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

                        var timings = new ReactionTimeData[trials.Trials.Length];

                        for (var i = 0; i < trials.Trials.Length; i++)
                        {
                            var trial = trials.Trials[i];
                            var trialData = _logData[trial];

                            var duration =
                                new TimeSpan(trialData.ReactionTime1).Subtract(new TimeSpan(trialData.StartTime1)).Milliseconds;

                            timings[i] = new ReactionTimeData
                            {
                                Duration = duration,
                                Time = new TimeSpan(trialData.StartTime1).Milliseconds
                            };
                        }

                        var result = trialTimings[^1];
                        result.ReactionTimes = timings;
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
            public int EventTimingIndex;
        }

        public struct TrialTiming
        {
            public NoiseLevel NoiseLevel;
            public LoadLevel LoadLevel;
            public ReactionTimeData[] ReactionTimes;
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
        
        public struct ReactionTimeData
        {
            public float Time;
            public float Duration;
        }
    }
}