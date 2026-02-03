using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.DependencyInjection.Abstractions;
using OpenFeature.Hosting;
using OpenFeature.Hosting.Internal;
using OpenFeature.Providers.DependencyInjection;
using Guard = OpenFeature.DependencyInjection.Abstractions.Guard;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="IServiceCollection"/> class.
/// </summary>
public static partial class OpenFeatureServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures OpenFeature services to the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configure">A configuration action for customizing OpenFeature setup via <see cref="OpenFeatureBuilder"/></param>
    /// <returns>The modified <see cref="IServiceCollection"/> instance</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configure"/> is null.</exception>
    public static IServiceCollection AddOpenFeature(this IServiceCollection services, Action<OpenFeatureBuilder> configure)
    {
        Guard.ThrowIfNull(services);
        Guard.ThrowIfNull(configure);

        // Register Hosting specific services
        services.TryAddSingleton(Api.Instance);
        services.TryAddSingleton<IFeatureLifecycleManager, FeatureLifecycleManager>();
        services.AddHostedService<HostedFeatureLifecycleService>();

        // Build OpenFeature components
        var builder = new OpenFeatureBuilder(services);
        configure(builder);

        var registory = builder.Build();
        var providerConfiguration = registory.GetProviderConfiguration();
        ConfigureClients(builder, providerConfiguration);

        return services;
    }

    private static void ConfigureClients(OpenFeatureBuilder builder, OpenFeatureProviderConfiguration configuration)
    {
        configuration.Validate();

        builder.AddClient();

        foreach (var domain in configuration.Domains)
        {
            builder.AddClient(domain);
        }

        if (configuration.IsPolicyConfigured)
        {
            builder.AddPolicyBasedClient();
        }
    }
}
