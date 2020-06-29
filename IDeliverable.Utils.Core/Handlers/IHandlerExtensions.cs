using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IDeliverable.Utils.Core.Handlers
{
    public static class IHandlerExtensions
    {
        public async static Task HandleAsync<TMessage>(this IEnumerable<IHandler<TMessage>> handlers, TMessage message, bool ignoreExceptions = false)
        {
            if (handlers == null)
                return;

            foreach (var handler in handlers)
            {
                try
                {
                    await handler.HandleAsync(message);
                }
                catch (Exception)
                {
                    if (!ignoreExceptions)
                        throw;
                }
            }
        }
    }
}

