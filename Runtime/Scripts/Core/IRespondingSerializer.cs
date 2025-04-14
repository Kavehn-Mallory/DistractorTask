namespace DistractorTask.Core
{
    public interface IRespondingSerializer<T> : ISerializer, IResponseIdentifier where T : IResponseIdentifier, ISerializer, new()
    {
        public T GenerateResponse();
    }
}