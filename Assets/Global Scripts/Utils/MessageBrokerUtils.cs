#if UNITASK_MESSAGEPIPE_SUPPORT
using MessagePipe;
using Cysharp.Threading.Tasks;

namespace GlobalScripts.Utils
{
    public struct AsyncMessage<T>
    {
        public T Data;
        public UniTaskCompletionSource<T> Source;
    }

    public struct MessageBrokerUtils<T>
    {
        public static UniTask<T> PublishAsyncMessage(IPublisher<AsyncMessage<T>> publisher, T data)
        {
            AsyncMessage<T> message = new AsyncMessage<T>
            {
                Data = data,
                Source = new()
            };

            publisher.Publish(message);
            return message.Source.Task;
        }

        public static bool SendBackMessage(AsyncMessage<T> message, T data)
        {
            if (message.Source == null)
                return false;

            return message.Source.TrySetResult(data);
        }
    }
}
#endif
