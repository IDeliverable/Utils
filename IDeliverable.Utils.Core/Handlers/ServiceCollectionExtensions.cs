using System;
using System.Linq;
using System.Threading.Tasks;
using IDeliverable.Utils.Core.Handlers;

namespace Microsoft.Extensions.DependencyInjection
{
	public static partial class ServiceCollectionExtensions
	{
		public static IServiceCollection AddHandler<TService>(this IServiceCollection services)
		{
			foreach (var implementedInterface in typeof(TService).GetInterfaces())
			{
                if (implementedInterface.IsConstructedGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IHandler<>))
                {
                    services.AddSingleton(implementedInterface, typeof(TService));
                    services.AddHandlers(implementedInterface);
                }
			}

			return services;
		}

		public static IServiceCollection AddHandler<TService>(this IServiceCollection services, TService implementationInstance)
		{
			foreach (var implementedInterface in typeof(TService).GetInterfaces())
			{
                if (implementedInterface.IsConstructedGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IHandler<>))
                {
                    services.AddSingleton(implementedInterface, implementationInstance);
                    services.AddHandlers(implementedInterface);
                }
            }

			return services;
		}

        public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Action<TMessage> handler)
        {
            var delegateHandler = new DelegateHandler<TMessage>(handler);

            services.AddSingleton<IHandler<TMessage>>(delegateHandler);
            services.AddHandlers(typeof(IHandler<TMessage>));

            return services;
        }

        public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Func<TMessage, Task> handler)
        {
            var delegateHandler = new DelegateHandler<TMessage>(handler);

            services.AddSingleton<IHandler<TMessage>>(delegateHandler);
            services.AddHandlers(typeof(IHandler<TMessage>));

            return services;
        }

        private static IServiceCollection AddHandlers(this IServiceCollection services, Type implementedInterface)
        {
            var messageType = implementedInterface.GenericTypeArguments[0];
            var handlersServiceType = typeof(IHandlers<>).MakeGenericType(messageType);

            // Only register the IHandlers<TMessage> once per message type.
            if (!services.Any(serviceDescription => serviceDescription.ServiceType == handlersServiceType))
            {
                var handlersImplementationType = typeof(Handlers<>).MakeGenericType(messageType);
                services.AddSingleton(handlersServiceType, handlersImplementationType);
            }

            return services;
        }
    }
}