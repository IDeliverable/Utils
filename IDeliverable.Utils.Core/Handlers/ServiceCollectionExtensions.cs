using System;
using IDeliverable.Utils.Handlers;

namespace Microsoft.Extensions.DependencyInjection
{
	public static partial class ServiceCollectionExtensions
	{
		public static IServiceCollection AddHandler<TService>(this IServiceCollection services)
		{
			foreach (var implementedInterface in typeof(TService).GetInterfaces())
			{
				if (implementedInterface.IsSubclassOfGeneric(typeof(IHandler<>)))
					services.AddSingleton(implementedInterface, typeof(TService));
			}

			return services;
		}

		public static IServiceCollection AddHandler<TService>(this IServiceCollection services, TService implementationInstance)
		{
			foreach (var implementedInterface in typeof(TService).GetInterfaces())
			{
				if (implementedInterface.IsSubclassOfGeneric(typeof(IHandler<>)))
					services.AddSingleton(implementedInterface, implementationInstance);
			}

			return services;
		}
	}
}