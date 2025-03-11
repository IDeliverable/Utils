using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IDeliverable.Utils.Core.Handlers;

namespace Microsoft.Extensions.DependencyInjection
{
	public static partial class ServiceCollectionExtensions
	{
		/// <summary>
		/// Registers a singleton service to be resolved and invoked to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// If the same <typeparamref name="TImplementation"/> type has already been registered, it will not be registered again.
		/// The resulting service registration will be resolvable both as <typeparamref name="TImplementation"/> and as any
		/// <see cref="IHandler{TMessage}"/> interfaces it implements.
		/// If <typeparamref name="TImplementation"/> implements <see cref="IHandler{TMessage}"/> for several TMessage types, a single
		/// <typeparamref name="TImplementation"/> instance will handle messages of all types.
		/// </remarks>
		/// 
		public static IServiceCollection AddHandler<TImplementation>(this IServiceCollection services) where TImplementation : class
		{
			GetHandlerInterfaces<TImplementation>();

			if (!services.Any(x => x.ServiceType == typeof(TImplementation) && x.ImplementationType == typeof(TImplementation)))
			{
				services.AddSingleton<TImplementation>();
			}

			services.AddHandler(serviceProvider => serviceProvider.GetRequiredService<TImplementation>());

			return services;
		}

		/// <summary>
		/// Registers a singleton instance to be invoked to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// If the same <paramref name="implementationInstance"/> instance has already been registered, it will not be registered again.
		/// The <paramref name="implementationInstance"/> will be resolvable both as <typeparamref name="TImplementation"/> and as any
		/// <see cref="IHandler{TMessage}"/> interfaces it implements.
		/// If <typeparamref name="TImplementation"/> implements <see cref="IHandler{TMessage}"/> for several TMessage types, the same
		/// <paramref name="implementationInstance"/> instance will handle messages of all types.
		/// </remarks>
		public static IServiceCollection AddHandler<TImplementation>(this IServiceCollection services, TImplementation implementationInstance) where TImplementation : class
		{
			GetHandlerInterfaces<TImplementation>();

			if (!services.Any(x => x.ServiceType == typeof(TImplementation) && x.ImplementationInstance == implementationInstance))
			{
				services.AddSingleton(implementationInstance);
			}

			services.AddHandler(serviceProvider => serviceProvider.GetRequiredService<TImplementation>());

			return services;
		}

		/// <summary>
		/// Registers a factory to be used to resolve instance to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// If the same <paramref name="implementationFactory"/> delegate has already been registered, it will not be registered again.
		/// If <typeparamref name="TImplementation"/> implements <see cref="IHandler{TMessage}"/> for several TMessage types, the same
		/// <paramref name="implementationFactory"/> will be called once per message type and may return the same
		/// <typeparamref name="TImplementation"/> instance or different instances.
		/// </remarks>
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

		/// <summary>
		/// Adds a delegate to be invoked to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// This registration method is not idempotent; calling it twice with the same <paramref name="handler"/> delegate will result in
		/// that delegate being invoked twice for every message of type <typeparamref name="TMessage"/>.
		/// </remarks>
		public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Action<TMessage> handler)
		{
			return services.AddSingleton<IHandler<TMessage>>(new DelegateHandler<TMessage>(handler));
		}

		/// <summary>
		/// Adds an async delegate to be invoked to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// This registration method is not idempotent; calling it twice with the same <paramref name="handler"/> delegate will result in
		/// that delegate being invoked twice for every message of type <typeparamref name="TMessage"/>.
		/// </remarks>
		public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Func<TMessage, Task> handler)
		{
			return services.AddSingleton<IHandler<TMessage>>(new DelegateHandler<TMessage>(handler));
		}

		/// <summary>
		/// Adds a cancellable delegate to be invoked to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// This registration method is not idempotent; calling it twice with the same <paramref name="handler"/> delegate will result in
		/// that delegate being invoked twice for every message of type <typeparamref name="TMessage"/>.
		/// </remarks>
		public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Action<TMessage, CancellationToken> handler)
		{
            return services.AddSingleton<IHandler<TMessage>>(new DelegateHandler<TMessage>(handler));
		}

		/// <summary>
		/// Adds an async cancellable delegate to be invoked to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// This registration method is not idempotent; calling it twice with the same <paramref name="handler"/> delegate will result in
		/// that delegate being invoked twice for every message of type <typeparamref name="TMessage"/>.
		/// </remarks>
		public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Func<TMessage, CancellationToken, Task> handler)
		{
            return services.AddSingleton<IHandler<TMessage>>(new DelegateHandler<TMessage>(handler));
		}

		/// <summary>
		/// Adds a delegate to be invoked with an <see cref="IServiceProvider"/> instance to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// This registration method is not idempotent; calling it twice with the same <paramref name="handler"/> delegate will result in
		/// that delegate being invoked twice for every message of type <typeparamref name="TMessage"/>.
		/// </remarks>
		public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Action<IServiceProvider, TMessage> handler)
		{
			return services.AddSingleton<IHandler<TMessage>>(serviceProvider => new DelegateHandler<TMessage>(message => handler(serviceProvider, message)));
		}

		/// <summary>
		/// Adds an async delegate to be invoked with an <see cref="IServiceProvider"/> instance to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// This registration method is not idempotent; calling it twice with the same <paramref name="handler"/> delegate will result in
		/// that delegate being invoked twice for every message of type <typeparamref name="TMessage"/>.
		/// </remarks>
		public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Func<IServiceProvider, TMessage, Task> handler)
		{
			return services.AddSingleton<IHandler<TMessage>>(serviceProvider => new DelegateHandler<TMessage>(message => handler(serviceProvider, message)));
		}

		/// <summary>
		/// Adds a cancellable delegate to be invoked with an <see cref="IServiceProvider"/> instance to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// This registration method is not idempotent; calling it twice with the same <paramref name="handler"/> delegate will result in
		/// that delegate being invoked twice for every message of type <typeparamref name="TMessage"/>.
		/// </remarks>
		public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Action<IServiceProvider, TMessage, CancellationToken> handler)
		{
			return services.AddSingleton<IHandler<TMessage>>(serviceProvider => new DelegateHandler<TMessage>((message, cancellationToken) => handler(serviceProvider, message, cancellationToken)));
		}

		/// <summary>
		/// Adds an async cancellable delegate to be invoked with an <see cref="IServiceProvider"/> instance to handle messages of type <typeparamref name="TMessage"/>.
		/// </summary>
		/// <remarks>
		/// This registration method is not idempotent; calling it twice with the same <paramref name="handler"/> delegate will result in
		/// that delegate being invoked twice for every message of type <typeparamref name="TMessage"/>.
		/// </remarks>
		public static IServiceCollection AddHandler<TMessage>(this IServiceCollection services, Func<IServiceProvider, TMessage, CancellationToken, Task> handler)
		{
			return services.AddSingleton<IHandler<TMessage>>(serviceProvider => new DelegateHandler<TMessage>((message, cancellationToken) => handler(serviceProvider, message, cancellationToken)));
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