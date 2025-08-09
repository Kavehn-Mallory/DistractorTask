using System;

namespace DistractorTask.Editor.UI
{
    [Serializable]
    public struct SerializableTimeSpan
    {
        public long serializedTimeSpan;

        public static implicit operator TimeSpan(SerializableTimeSpan timeSpan) =>
            new TimeSpan(timeSpan.serializedTimeSpan);

        public static implicit operator SerializableTimeSpan(TimeSpan timeSpan) => new SerializableTimeSpan
        {
            serializedTimeSpan = timeSpan.Ticks
        };
    }
}