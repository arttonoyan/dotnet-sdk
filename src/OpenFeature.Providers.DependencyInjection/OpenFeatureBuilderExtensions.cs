using Microsoft.Extensions.DependencyInjection;
using OpenFeature.DependencyInjection.Abstractions;
using OpenFeature.Providers.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="OpenFeatureBuilder"/> class.
/// </summary>
#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(Providers.DependencyInjection.Diagnostics.FeatureCodes.NewDi)]
#endif
public static partial class OpenFeatureBuilderExtensions
{
    /// <summary>
    /// Adds a feature provider using a factory method without additional configuration options.
    /// This method adds the feature provider as a transient service and sets it as the default provider within the application.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates and returns a <see cref="FeatureProvider"/>
    /// instance based on the provided service provider.
    /// </param>
    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the default feature provider set and configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is null, as a valid builder is required to add and configure providers.</exception>
    public static OpenFeatureBuilder AddProvider(this OpenFeatureBuilder builder, Func<IServiceProvider, FeatureProvider> implementationFactory)
        => AddProvider<OpenFeatureProviderOptions>(builder, implementationFactory, null);

    /// <summary>
    /// Adds a feature provider using a factory method to create the provider instance and optionally configures its settings.
    /// This method adds the feature provider as a transient service and sets it as the default provider within the application.
    /// </summary>
    /// <typeparam name="TOptions"> Type derived from <see cref="OpenFeatureBuilder"/> used to configure the feature provider.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates and returns a <see cref="FeatureProvider"/>
    /// instance based on the provided service provider.
    /// </param>
    /// <param name="configureOptions">An optional delegate to configure the provider-specific options.</param>
    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the default feature provider set and configured.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is null, as a valid builder is required to add and configure providers.</exception>
    public static OpenFeatureBuilder AddProvider<TOptions>(this OpenFeatureBuilder builder, Func<IServiceProvider, FeatureProvider> implementationFactory, Action<TOptions>? configureOptions)
        where TOptions : OpenFeatureProviderOptions
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

        if (configureOptions is not null)
        {
            builder.Services.Configure(configureOptions);
        }

        return builder.AddComponent(new DefaultProviderComponent(implementationFactory));
    }

    /// <summary>
    /// Adds a feature provider for a specific domain using provided options and a configuration builder.
    /// </summary>
    /// <typeparam name="TOptions"> Type derived from <see cref="OpenFeatureBuilder"/> used to configure the feature provider.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
    /// <param name="domain">The unique name of the provider.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates a feature provider instance.
    /// It adds the provider as a transient service unless it is already added.
    /// </param>
    /// <param name="configureOptions">An optional delegate to configure the provider-specific options.</param>
    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the new feature provider configured.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="builder"/> or <paramref name="domain"/> is null or if the <paramref name="domain"/> is empty.
    /// </exception>
    public static OpenFeatureBuilder AddProvider<TOptions>(this OpenFeatureBuilder builder, string domain, Func<IServiceProvider, string, FeatureProvider> implementationFactory, Action<TOptions>? configureOptions)
        where TOptions : OpenFeatureProviderOptions
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

        //if (string.IsNullOrWhiteSpace(domain))
        //    throw new ArgumentException("Domain is required.", nameof(domain));

        if (configureOptions is not null)
        {
            builder.Services.Configure(domain, configureOptions);
        }

        return builder.AddComponent(new NamedProviderComponent(domain, implementationFactory));
    }

    /// <summary>
    /// Adds a feature provider for a specified domain using the default options.
    /// This method configures a feature provider without custom options, delegating to the more generic AddProvider method.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
    /// <param name="domain">The unique name of the provider.</param>
    /// <param name="implementationFactory">
    /// A factory method that creates a feature provider instance.
    /// It adds the provider as a transient service unless it is already added.
    /// </param>
    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the new feature provider configured.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if either <paramref name="builder"/> or <paramref name="domain"/> is null or if the <paramref name="domain"/> is empty.
    /// </exception>
    public static OpenFeatureBuilder AddProvider(this OpenFeatureBuilder builder, string domain, Func<IServiceProvider, string, FeatureProvider> implementationFactory)
        => AddProvider<OpenFeatureProviderOptions>(builder, domain, implementationFactory, configureOptions: null);

    /// <summary>
    /// Configures policy name options for OpenFeature using the specified options type.
    /// </summary>
    /// <typeparam name="TOptions">The type of options used to configure <see cref="OpenFeatureProviderOptions"/>.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureOptions">A delegate to configure <typeparamref name="TOptions"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="configureOptions"/> is null.</exception>
    public static OpenFeatureBuilder AddPolicyName<TOptions>(this OpenFeatureBuilder builder, Action<TOptions> configureOptions)
        where TOptions : PolicyNameOptions
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        builder.Services.AddOptions<TOptions>();
        builder.Services.Configure(configureOptions);
        return builder.AddComponent(new ProviderPolicyComponent());
    }

    /// <summary>
    /// Configures the default policy name options for OpenFeature.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureOptions">A delegate to configure <see cref="OpenFeatureBuilder"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddPolicyName(this OpenFeatureBuilder builder, Action<PolicyNameOptions> configureOptions)
        => AddPolicyName<PolicyNameOptions>(builder, configureOptions);

}
