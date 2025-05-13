namespace DistractorTask.Core
{
    public interface IResponseIdentifier : ISerializer
    {
        /// <summary>
        /// Unique Id that identifies the sender (e.g. GetInstanceId())
        /// </summary>
        public int SenderId { get; set; }
        /// <summary>
        /// Id that identifies the message and response, this could be an index that is incremented after every call but could also be freely chosen. It allows to identify the target of the response 
        /// </summary>
        public int MessageId { get; set; }
    }
}