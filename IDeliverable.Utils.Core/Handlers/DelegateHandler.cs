using System;
using System.Threading.Tasks;

namespace IDeliverable.Utils.Core.Handlers
{
    public class DelegateHandler<TMessage> : IHandler<TMessage>
    {
        public DelegateHandler(Action<TMessage> handler)
        {
            mHandler = message =>
            {
                handler(message);
                return Task.CompletedTask;
            };
        }

        public DelegateHandler(Func<TMessage, Task> handler)
        {
            mHandler = handler;
        }

        private readonly Func<TMessage, Task> mHandler;

        public Task HandleAsync(TMessage message)
        {
            return mHandler(message);
        }
    }
}
