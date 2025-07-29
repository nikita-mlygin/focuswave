using Focuswave.Common.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Focuswave.Common;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddEventDispatcher(this IServiceCollection sc)
    {
        sc.AddScoped<IEventDispatcher, InMemoryDispatcher>();

        return sc;
    }
}
