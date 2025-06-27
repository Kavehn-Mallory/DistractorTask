using System;

namespace DistractorTask.UserStudy.Core
{
    [Serializable]
    public struct Condition
    {
        public LoadLevel loadLevels;
        public NoiseLevel noiseLevels;
        public bool hasAudioTask;
    }

    [Serializable, Flags]
    public enum LoadLevel
    {
        Low = 1 << 0,
        High = 1 << 1
    }

    [Serializable, Flags]
    public enum NoiseLevel
    {
        None = 1 << 0,
        Low = 1 << 1,
        Audio = 1 << 2,
        Visual = 1 << 3,
        High = 1 << 4
    }
}