using System;
using System.Threading;
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
                    _ = services.AddSingleton(implementedInterface, typeof(TService));
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
                    _ = services.AddSingleton(implementedInterface, implementationInstance);
                }
            }

            return services;
        }

        public static IServiceCollection AddHandler<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            foreach (var implementedInterface in typeof(TService).GetInterfaces())
            {
                if (implementedInterface.IsConstructedGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(IHandler<>))
                {
                    _ = services.AddSingleton(implementedInterface, implementationFactory);
                }
            }

            return services;
        }

        public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Action<TMessage> handler)
        {
            var delegateHandler = new DelegateHandler<TMessage>(handler);

            return services.AddSingleton<IHandler<TMessage>>(delegateHandler);
        }

        public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Func<TMessage, Task> handler)
        {
            var delegateHandler = new DelegateHandler<TMessage>(handler);

            return services.AddSingleton<IHandler<TMessage>>(delegateHandler);
        }

        public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Action<TMessage, CancellationToken> handler)
        {
            var delegateHandler = new DelegateHandler<TMessage>(handler);

            return services.AddSingleton<IHandler<TMessage>>(delegateHandler);
        }

        public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Func<TMessage, CancellationToken, Task> handler)
        {
            var delegateHandler = new DelegateHandler<TMessage>(handler);

            return services.AddSingleton<IHandler<TMessage>>(delegateHandler);
        }

        public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Func<IServiceProvider, TMessage, CancellationToken, Task> handler)
        {
            return services.AddSingleton<IHandler<TMessage>>((services) => new DelegateHandler<TMessage>((mesage, cancellationToken) => handler(services, mesage, cancellationToken)));
        }
    }
}