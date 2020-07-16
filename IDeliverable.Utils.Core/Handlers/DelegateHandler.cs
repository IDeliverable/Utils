using System;
using System.Threading;
using System.Threading.Tasks;

namespace IDeliverable.Utils.Core.Handlers
{
    public class DelegateHandler<TMessage> : IHandler<TMessage>
    {
        public DelegateHandler(Action<TMessage> handler)
        {
            mHandler = (message, _) =>
            {
                handler(message);
                return Task.CompletedTask;
            };
        }

        public DelegateHandler(Action<TMessage, CancellationToken> handler)
        {
            mHandler = (message, cancellationToken) =>
            {
                handler(message, cancellationToken);
                return Task.CompletedTask;
            };
        }

        public DelegateHandler(Func<TMessage, Task> handler)
        {
            mHandler = (message, _) =>
            {
                handler(message);
                return Task.CompletedTask;
            };
        }

        public DelegateHandler(Func<TMessage, CancellationToken, Task> handler)
        {
            mHandler = handler;
        }

        private readonly Func<TMessage, CancellationToken, Task> mHandler;

        public Task HandleAsync(TMessage message, CancellationToken cancellationToken = default)
        {
            return mHandler(message, cancellationToken);
        }
    }
}
