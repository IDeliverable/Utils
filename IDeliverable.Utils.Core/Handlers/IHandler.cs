using System;
using System.Collections.Generic;

namespace IDeliverable.Utils.Handlers
{
	public interface IHandler<TMessage>
	{
		void Handle(TMessage message);
	}

	public static class IHandlerExtensions
	{
		public static void Handle<TMessage>(this IEnumerable<IHandler<TMessage>> handlers, TMessage message, bool ignoreExceptions = false)
		{
			if (handlers == null)
				return;

			foreach (var handler in handlers)
			{
				try
				{
					handler.Handle(message);
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

