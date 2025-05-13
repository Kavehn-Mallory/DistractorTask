using DistractorTask.Core;

namespace DistractorTask.Transport.DataContainer.GenericClasses
{
    public abstract class BaseRespondingData<T> : BaseResponseData, IRespondingSerializer<T>
        where T : ISerializer, IResponseIdentifier, new()
    {
        
        public virtual T GenerateResponse()
        {
            var response = new T
            {
                MessageId = MessageId,
                SenderId = SenderId
            };
            return response;
        }

        public T GenerateResponse(T value)
        {
            return GenerateResponse();
        }
    }

}