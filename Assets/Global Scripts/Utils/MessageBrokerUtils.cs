#if UNITASK_MESSAGEPIPE_SUPPORT
using MessagePipe;
using Cysharp.Threading.Tasks;

namespace GlobalScripts.Utils
{
    public struct AsyncMessage<TData>
    {
        public TData Data;
        public UniTaskCompletionSource<TData> Source;
    }

    public struct MessageBrokerUtils<TMessageData>
    {
        public static UniTask<TMessageData> PublishAsyncMessage(IPublisher<AsyncMessage<TMessageData>> publisher, TMessageData data)
        {
            AsyncMessage<TMessageData> message = new AsyncMessage<TMessageData>
            {
                Data = data,
                Source = new()
            };

            publisher.Publish(message);
            return message.Source.Task;
        }

        public static bool SendBackMessage(AsyncMessage<TMessageData> message, TMessageData data)
        {
            return message.Source == null ? false : message.Source.TrySetResult(data);
        }
    }
}
#endif
