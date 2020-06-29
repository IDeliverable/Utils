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
                    _ = services
                        .AddSingleton(implementedInterface, typeof(TService))
                        .AddHandlers(implementedInterface);
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
                    _ = services
                        .AddSingleton(implementedInterface, implementationInstance)
                        .AddHandlers(implementedInterface);
                }
            }

            return services;
        }

        public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Action<TMessage> handler)
        {
            var delegateHandler = new DelegateHandler<TMessage>(handler);

            return services
                .AddSingleton<IHandler<TMessage>>(delegateHandler)
                .AddHandlers(typeof(IHandler<TMessage>));
        }

        public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Func<TMessage, Task> handler)
        {
            var delegateHandler = new DelegateHandler<TMessage>(handler);

            return services
                .AddSingleton<IHandler<TMessage>>(delegateHandler)
                .AddHandlers(typeof(IHandler<TMessage>));
        }

        private static IServiceCollection AddHandlers(this IServiceCollection services, Type implementedInterface)
        {
            var messageType = implementedInterface.GenericTypeArguments[0];
            var handlersServiceType = typeof(IHandlers<>).MakeGenericType(messageType);

            // Only register the IHandlers<TMessage> once per message type.
            if (!services.Any(serviceDescription => serviceDescription.ServiceType == handlersServiceType))
            {
                var handlersImplementationType = typeof(Handlers<>).MakeGenericType(messageType);
                _ = services.AddSingleton(handlersServiceType, handlersImplementationType);
            }

            return services;
        }
    }
}