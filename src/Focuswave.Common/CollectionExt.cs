using Focuswave.Common.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Focuswave.Common;

public static class ServiceCollectionExt
{
    /// <summary>
    /// Adds the event dispatcher to the service collection.
    /// </summary>
    /// <param name="sc">The IServiceCollection to add the dispatcher to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddEventDispatcher(this IServiceCollection sc)
    {
        sc.AddScoped<IEventDispatcher, InMemoryDispatcher>();

        return sc;
    }
}
