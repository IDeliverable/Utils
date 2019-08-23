using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IDeliverable.Utils.Core.Handlers
{
    class Handlers<TMessage> : IHandlers<TMessage>
    {
        public Handlers(IEnumerable<IHandler<TMessage>> handlers)
        {
            mHandlers = handlers;
        }

        private readonly IEnumerable<IHandler<TMessage>> mHandlers;

        public async Task HandleAsync(TMessage message, bool ignoreExceptions = false)
        {
            if (mHandlers == null)
                return;

            foreach (var handler in mHandlers)
            {
                try
                {
                    await handler.HandleAsync(message);
                }
                catch (Exception ex)
                {
                    if (!ignoreExceptions)
                        throw ex;
                }
            }
        }
    }
}
