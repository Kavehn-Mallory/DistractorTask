namespace DistractorTask.Core
{
    public interface IStudyStageEvent : ISerializer
    {
        public bool IsStartEvent { get; set; }
    }
}