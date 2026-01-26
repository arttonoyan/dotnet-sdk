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
    ///// <summary>
    ///// Adds and configures OpenFeature services to the provided <see cref="IServiceCollection"/>.
    ///// </summary>
    ///// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    ///// <param name="configure">A configuration action for customizing OpenFeature setup via <see cref="OpenFeatureBuilder"/></param>
    ///// <returns>The modified <see cref="IServiceCollection"/> instance</returns>
    ///// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configure"/> is null.</exception>
    //public static IServiceCollection AddOpenFeature(this IServiceCollection services, Action<OpenFeatureBuilder> configure)
    //{
    //    Guard.ThrowIfNull(services);
    //    Guard.ThrowIfNull(configure);

    //    // Register core OpenFeature services as singletons.
    //    services.TryAddSingleton(Api.Instance);
    //    services.TryAddSingleton<IFeatureLifecycleManager, FeatureLifecycleManager>();

    //    var builder = new OpenFeatureBuilder(services);
    //    configure(builder);

    //    builder.Services.Configure<OpenFeatureOptions>(c => { }); // Ensures IOptions<OpenFeatureOptions> is available even when no providers are configured.
    //    builder.Services.AddHostedService<HostedFeatureLifecycleService>();

    //    // If a default provider is specified without additional providers,
    //    // return early as no extra configuration is needed.
    //    if (builder.HasDefaultProvider && builder.DomainBoundProviderRegistrationCount == 0)
    //    {
    //        return services;
    //    }

    //    // Validate builder configuration to ensure consistency and required setup.
    //    builder.Validate();

    //    if (!builder.IsPolicyConfigured)
    //    {
    //        // Add a default name selector policy to use the first registered provider name as the default.
    //        builder.AddPolicyName(options =>
    //        {
    //            options.DefaultNameSelector = provider =>
    //            {
    //                var options = provider.GetRequiredService<IOptions<OpenFeatureProviderOptions>>().Value;
    //                return options.ProviderNames.FirstOrDefault();
    //            };
    //        });
    //    }

    //    builder.AddPolicyBasedClient();

    //    return services;
    //}

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

        // Register core OpenFeature services as singletons.
        services.TryAddSingleton(Api.Instance);
        services.TryAddSingleton<IFeatureLifecycleManager, FeatureLifecycleManager>();

        services.AddOptions<OpenFeatureOptions>();
        services.AddOptions<OpenFeatureProviderOptions>();
        // services.Configure<OpenFeatureOptions>(c => { }); // Ensures IOptions<OpenFeatureOptions> is available even when no providers are configured.
        
        var builder = new OpenFeatureBuilder(services);
        configure(builder);

        // Finalize components without building the container
        var context = builder.CreateContext();
        foreach (var finalizer in builder.Components.OfType<IOpenFeatureFinalizer>())
        {
            finalizer.Finalize(context);
        }

        services.AddHostedService<HostedFeatureLifecycleService>();

        return services;
    }

    //internal static OpenFeatureBuilder AddPolicyBasedClient(this OpenFeatureBuilder builder)
    //{
    //    builder.Services.AddScoped(provider =>
    //    {
    //        var policy = provider.GetRequiredService<IOptions<PolicyNameOptions>>().Value;
    //        var name = policy.DefaultNameSelector(provider);
    //        return ResolveFeatureClient(provider, name);
    //    });

    //    return builder;
    //}

    //private static IFeatureClient ResolveFeatureClient(IServiceProvider provider, string? name = null)
    //{
    //    var api = provider.GetRequiredService<Api>();
    //    var client = api.GetClient(name);
    //    var context = provider.GetService<EvaluationContext>();
    //    if (context != null)
    //    {
    //        client.SetContext(context);
    //    }

    //    return client;
    //}
}
