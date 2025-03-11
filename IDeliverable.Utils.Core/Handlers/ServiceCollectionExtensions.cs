using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IDeliverable.Utils.Core.Handlers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
		/// <remarks>This registration method is idempotent; if the same <typeparamref name="TImplementation"/> type has already been registered as a handler it will not be registered again.</remarks>
        public static IServiceCollection AddHandler<TImplementation>(this IServiceCollection services) where TImplementation : class
        {
            foreach (var handlerInterface in GetHandlerInterfaces<TImplementation>())
            {
				if (!services.Any(x => x.ServiceType == handlerInterface && x.ImplementationType == typeof(TImplementation)))
				{
					services.AddSingleton(handlerInterface, typeof(TImplementation));
				}
            }

            return services;
        }

		/// <remarks>This registration method is idempotent; if the same <paramref name="implementationInstance"/> instance has already been registered as a handler it will not be registered again.</remarks>
        public static IServiceCollection AddHandler<TImplementation>(this IServiceCollection services, TImplementation implementationInstance) where TImplementation : class
        {
            foreach (var handlerInterface in GetHandlerInterfaces<TImplementation>())
            {
				if (!services.Any(x => x.ServiceType == handlerInterface && x.ImplementationInstance == implementationInstance))
				{
					services.AddSingleton(handlerInterface, implementationInstance);
				}
            }

            return services;
        }

		/// <remarks>This registration method is idempotent; if the same <paramref name="implementationFactory"/> delegate has already been registered it will not be registered again.</remarks>
        public static IServiceCollection AddHandler<TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory) where TImplementation : class
        {
            foreach (var handlerInterface in GetHandlerInterfaces<TImplementation>())
            {
				if (!services.Any(x => x.ServiceType == handlerInterface && x.ImplementationFactory == implementationFactory))
				{
					services.AddSingleton(handlerInterface, implementationFactory);
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

		private static Type[] GetHandlerInterfaces<TImplementation>()
		{
			var handlerInterfaces =
				typeof(TImplementation).GetInterfaces()
					.Where(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IHandler<>))
					.ToArray();

			if (!handlerInterfaces.Any())
			{
				throw new ArgumentException($"Generic type parameter {nameof(TImplementation)} must refer to a type that implements IHandler<TMessage> for at least one TMessage type.");
			}

			return handlerInterfaces;
		} 
    }
}