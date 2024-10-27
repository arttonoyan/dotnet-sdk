using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.DependencyInjection;
using OpenFeature.DependencyInjection.Internal;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="IServiceCollection"/> class.
/// </summary>
public static partial class OpenFeatureServiceCollectionExtensions
{
    /// <summary>
    /// This method is used to add OpenFeature to the service collection.
    /// OpenFeature will be registered as a singleton.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>the current <see cref="IServiceCollection"/> instance</returns>
    public static IServiceCollection AddOpenFeature(this IServiceCollection services, Action<OpenFeatureBuilder> configure)
    {
        Guard.ThrowIfNull(services);
        Guard.ThrowIfNull(configure);

        services.TryAddSingleton(Api.Instance);
        services.TryAddSingleton<IFeatureLifecycleManager, FeatureLifecycleManager>();

        var builder = new OpenFeatureBuilder(services);
        configure(builder);


        builder.AddDefaultClient((provider, policy) =>
        {
            if (policy.DefaultNameSelector == null)
            {
                return provider.GetRequiredService<IFeatureClient>();
            }

            var name = policy.DefaultNameSelector.Invoke(provider);
            if (name == null)
            {
                return provider.GetRequiredService<IFeatureClient>();
            }

            return provider.GetRequiredKeyedService<IFeatureClient>(name);
        });

        return services;
    }
}
