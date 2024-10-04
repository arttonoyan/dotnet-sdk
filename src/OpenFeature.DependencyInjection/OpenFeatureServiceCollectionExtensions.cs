using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.Internal;
using OpenFeature.Model;
using System.Diagnostics.CodeAnalysis;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="IServiceCollection"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public static class OpenFeatureServiceCollectionExtensions
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
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.TryAddSingleton(Api.Instance);
        services.TryAddSingleton<IFeatureLifecycleManager, FeatureLifecycleManager>();

        var builder = new OpenFeatureBuilder(services);
        configure(builder);

        if (builder.IsContextConfigured)
        {
            services.TryAddScoped<IFeatureClient>(static provider => {
                var api = provider.GetRequiredService<Api>();
                var client = api.GetClient();
                var context = provider.GetRequiredService<EvaluationContext>();
                client.SetContext(context);
                return client;
            });
        }
        else
        {
            services.TryAddScoped<IFeatureClient>(static provider => {
                var api = provider.GetRequiredService<Api>();
                return api.GetClient();
            });
        }

        return services;
    }
}
