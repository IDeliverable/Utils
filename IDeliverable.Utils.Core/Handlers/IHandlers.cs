using System;
using System.Threading.Tasks;

namespace IDeliverable.Utils.Core.Handlers
{
    public interface IHandlers<TMessage>
    {
        Task HandleAsync(TMessage message, bool ignoreExceptions = false);
    }
}
