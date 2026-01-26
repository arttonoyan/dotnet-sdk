using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenFeature.DependencyInjection.Abstractions;
using OpenFeature.Providers.DependencyInjection;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="OpenFeatureBuilder"/> class.
/// </summary>
#if NET8_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.Experimental(Providers.DependencyInjection.Diagnostics.FeatureCodes.NewDi)]
#endif
public static partial class OpenFeatureProviderBuilderExtensions
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
        //if (builder == null) throw new ArgumentNullException(nameof(builder));

        ////builder.HasDefaultProvider = true;
        //builder.Services.PostConfigure<TOptions>(options =>
        //{
        //    options.HasDefaultProvider = true;
        //    options.AddDefaultProviderName();
        //});
        //if (configureOptions != null)
        //{
        //    builder.Services.Configure(configureOptions);
        //}

        //builder.Services.TryAddTransient(implementationFactory);
        //builder.AddClient();
        //return builder;
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

        builder.Services.AddOptions<OpenFeatureProviderOptions>();
        builder.Services.PostConfigure<OpenFeatureProviderOptions>(o =>
        {
            o.HasDefaultProvider = true;
            o.AddDefaultProviderName();
        });

        if (configureOptions is not null)
        {
            builder.Services.Configure(configureOptions);
        }

        builder.Services.TryAddScoped<IFeatureClient>(static provider =>
        {
            var api = provider.GetRequiredService<Api>();
            var client = api.GetClient();

            var context = provider.GetService<EvaluationContext>();
            if (context is not null)
            {
                client.SetContext(context);
            }

            return client;
        });

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
        //if (builder == null) throw new ArgumentNullException(nameof(builder));

        ////builder.DomainBoundProviderRegistrationCount++;

        //builder.Services.PostConfigure<TOptions>(domain, options =>
        //{
        //    options.DomainBoundProviderRegistrationCount++;
        //    options.AddProviderName(domain);
        //});
        //if (configureOptions != null)
        //{
        //    builder.Services.Configure(domain, configureOptions);
        //}

        //builder.Services.TryAddKeyedTransient(domain, (provider, key) =>
        //{
        //    if (key == null)
        //    {
        //        throw new ArgumentNullException(nameof(key));
        //    }
        //    return implementationFactory(provider, key.ToString()!);
        //});

        //builder.AddClient(domain);
        //return builder;
        if (builder == null) throw new ArgumentNullException(nameof(builder));
        if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain is required.", nameof(domain));

        builder.Services.AddOptions<OpenFeatureProviderOptions>();
        builder.Services.PostConfigure<OpenFeatureProviderOptions>(o =>
        {
            o.DomainBoundProviderRegistrationCount++;
            o.AddProviderName(domain);
        });

        if (configureOptions is not null)
        {
            builder.Services.Configure(domain, configureOptions);
        }

        builder.Services.TryAddKeyedScoped<IFeatureClient>(domain, static (provider, key) =>
        {
            var api = provider.GetRequiredService<Api>();
            var client = api.GetClient(key!.ToString());

            var context = provider.GetService<EvaluationContext>();
            if (context is not null)
            {
                client.SetContext(context);
            }

            return client;
        });

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

        //builder.IsPolicyConfigured = true;

        builder.Services.AddOptions<TOptions>();
        builder.Services.Configure(configureOptions);
        //builder.Services.Configure(configureOptions).PostConfigure<OpenFeatureProviderOptions>(options =>
        //{
        //    options.IsPolicyConfigured = true;
        //});

        //return builder;
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

    private static OpenFeatureBuilder EnsureDefaultPolicyName(this OpenFeatureBuilder builder)
    {
        builder.Services.AddOptions<PolicyNameOptions>();
        return builder.AddComponent(new ProviderPolicyComponent());
    }
}

internal sealed class ProviderPolicyComponent : IOpenFeatureComponent, IOpenFeatureFinalizer
{
    public void Register(OpenFeatureComponentContext context)
    {
        context.Services.AddOptions<PolicyNameOptions>();
        context.Services.AddOptions<OpenFeatureProviderOptions>();
    }

    public void Finalize(OpenFeatureComponentContext context)
    {
        context.Services.PostConfigure<PolicyNameOptions>(options =>
        {
            if (options.DefaultNameSelector is not null)
            {
                return;
            }

            options.DefaultNameSelector = sp =>
            {
                var providerOptions = sp.GetRequiredService<IOptions<OpenFeatureProviderOptions>>().Value;
                return providerOptions.ProviderNames.FirstOrDefault();
            };
        });
    }
}

internal sealed class DefaultProviderComponent(Func<IServiceProvider, FeatureProvider> factory) : IOpenFeatureComponent
{
    private readonly Func<IServiceProvider, FeatureProvider> _factory = factory;

    public void Register(OpenFeatureComponentContext context)
        => context.Services.TryAddTransient(_factory);
}

internal sealed class NamedProviderComponent(
    string name,
    Func<IServiceProvider, string, FeatureProvider> factory) : IOpenFeatureComponent
{
    private readonly string _name = name;
    private readonly Func<IServiceProvider, string, FeatureProvider> _factory = factory;

    public void Register(OpenFeatureComponentContext context)
    {
        context.Services.TryAddKeyedTransient(_name, (sp, key) =>
        {
            var k = key?.ToString();
            if (string.IsNullOrWhiteSpace(k))
                throw new ArgumentException("Provider name must not be empty.", nameof(key));

            return _factory(sp, k!);
        });
    }
}

internal static class ProviderPolicyDefaults
{
    public static void EnsureDefaultNameSelector(IServiceCollection services)
    {
        services.AddOptions<PolicyNameOptions>();
        services.PostConfigure<PolicyNameOptions>(options =>
        {
            options.DefaultNameSelector ??= sp =>
            {
                var providerOptions = sp.GetRequiredService<IOptions<OpenFeatureProviderOptions>>().Value;
                return providerOptions.ProviderNames.FirstOrDefault();
            };
        });
    }
}

internal sealed class ProviderRegistryState
{
    public bool HasDefault { get; set; }
    public int NamedCount { get; set; }
    public HashSet<string> Names { get; } = new(StringComparer.Ordinal);
}

internal static class ProviderRegistryStateExtensions
{
    public static ProviderRegistryState GetProviderState(this OpenFeatureComponentContext context)
        => context.GetOrAddState(static () => new ProviderRegistryState());
}

internal sealed class ProviderPolicyFinalizer : IOpenFeatureComponent, IOpenFeatureFinalizer
{
    public void Register(OpenFeatureComponentContext context)
    {
        context.Services.AddOptions<PolicyNameOptions>();
    }

    public void Finalize(OpenFeatureComponentContext context)
    {
        context.Services.PostConfigure<PolicyNameOptions>(options =>
        {
            if (options.DefaultNameSelector is not null)
            {
                return;
            }

            options.DefaultNameSelector = sp =>
            {
                var providerOptions = sp.GetRequiredService<IOptions<OpenFeatureProviderOptions>>().Value;
                return providerOptions.ProviderNames.FirstOrDefault();
            };
        });
    }
}



///// <summary>
///// Contains extension methods for the <see cref="OpenFeatureBuilder"/> class.
///// </summary>
//#if NET8_0_OR_GREATER
//[System.Diagnostics.CodeAnalysis.Experimental(Providers.DependencyInjection.Diagnostics.FeatureCodes.NewDi)]
//#endif
//public static partial class OpenFeatureProviderBuilderExtensions
//{
//    /// <summary>
//    /// Adds a feature provider using a factory method without additional configuration options.
//    /// This method adds the feature provider as a transient service and sets it as the default provider within the application.
//    /// </summary>
//    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
//    /// <param name="implementationFactory">
//    /// A factory method that creates and returns a <see cref="FeatureProvider"/>
//    /// instance based on the provided service provider.
//    /// </param>
//    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the default feature provider set and configured.</returns>
//    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is null, as a valid builder is required to add and configure providers.</exception>
//    public static OpenFeatureBuilder AddProvider(this OpenFeatureBuilder builder, Func<IServiceProvider, FeatureProvider> implementationFactory)
//        => AddProvider<OpenFeatureProviderOptions>(builder, implementationFactory, null);

//    /// <summary>
//    /// Adds a feature provider using a factory method to create the provider instance and optionally configures its settings.
//    /// This method adds the feature provider as a transient service and sets it as the default provider within the application.
//    /// </summary>
//    /// <typeparam name="TOptions"> Type derived from <see cref="OpenFeatureBuilder"/> used to configure the feature provider.</typeparam>
//    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
//    /// <param name="implementationFactory">
//    /// A factory method that creates and returns a <see cref="FeatureProvider"/>
//    /// instance based on the provided service provider.
//    /// </param>
//    /// <param name="configureOptions">An optional delegate to configure the provider-specific options.</param>
//    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the default feature provider set and configured.</returns>
//    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="builder"/> is null, as a valid builder is required to add and configure providers.</exception>
//    public static OpenFeatureBuilder AddProvider<TOptions>(this OpenFeatureBuilder builder, Func<IServiceProvider, FeatureProvider> implementationFactory, Action<TOptions>? configureOptions)
//        where TOptions : OpenFeatureProviderOptions
//    {
//        if (builder == null) throw new ArgumentNullException(nameof(builder));

//        //builder.HasDefaultProvider = true;
//        builder.Services.PostConfigure<TOptions>(options =>
//        {
//            options.HasDefaultProvider = true;
//            options.AddDefaultProviderName();
//        });
//        if (configureOptions != null)
//        {
//            builder.Services.Configure(configureOptions);
//        }

//        builder.Services.TryAddTransient(implementationFactory);
//        builder.AddClient();
//        return builder;
//    }

//    /// <summary>
//    /// Adds a feature provider for a specific domain using provided options and a configuration builder.
//    /// </summary>
//    /// <typeparam name="TOptions"> Type derived from <see cref="OpenFeatureBuilder"/> used to configure the feature provider.</typeparam>
//    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
//    /// <param name="domain">The unique name of the provider.</param>
//    /// <param name="implementationFactory">
//    /// A factory method that creates a feature provider instance.
//    /// It adds the provider as a transient service unless it is already added.
//    /// </param>
//    /// <param name="configureOptions">An optional delegate to configure the provider-specific options.</param>
//    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the new feature provider configured.</returns>
//    /// <exception cref="ArgumentNullException">
//    /// Thrown if either <paramref name="builder"/> or <paramref name="domain"/> is null or if the <paramref name="domain"/> is empty.
//    /// </exception>
//    public static OpenFeatureBuilder AddProvider<TOptions>(this OpenFeatureBuilder builder, string domain, Func<IServiceProvider, string, FeatureProvider> implementationFactory, Action<TOptions>? configureOptions)
//        where TOptions : OpenFeatureProviderOptions
//    {
//        if (builder == null) throw new ArgumentNullException(nameof(builder));

//        //builder.DomainBoundProviderRegistrationCount++;

//        builder.Services.PostConfigure<TOptions>(options =>
//        {
//            options.DomainBoundProviderRegistrationCount++;
//            options.AddProviderName(domain);
//        });
//        if (configureOptions != null)
//        {
//            builder.Services.Configure(domain, configureOptions);
//        }

//        builder.Services.TryAddKeyedTransient(domain, (provider, key) =>
//        {
//            if (key == null)
//            {
//                throw new ArgumentNullException(nameof(key));
//            }
//            return implementationFactory(provider, key.ToString()!);
//        });

//        builder.AddClient(domain);
//        return builder;
//    }

//    /// <summary>
//    /// Adds a feature provider for a specified domain using the default options.
//    /// This method configures a feature provider without custom options, delegating to the more generic AddProvider method.
//    /// </summary>
//    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> used to configure feature flags.</param>
//    /// <param name="domain">The unique name of the provider.</param>
//    /// <param name="implementationFactory">
//    /// A factory method that creates a feature provider instance.
//    /// It adds the provider as a transient service unless it is already added.
//    /// </param>
//    /// <returns>The updated <see cref="OpenFeatureBuilder"/> instance with the new feature provider configured.</returns>
//    /// <exception cref="ArgumentNullException">
//    /// Thrown if either <paramref name="builder"/> or <paramref name="domain"/> is null or if the <paramref name="domain"/> is empty.
//    /// </exception>
//    public static OpenFeatureBuilder AddProvider(this OpenFeatureBuilder builder, string domain, Func<IServiceProvider, string, FeatureProvider> implementationFactory)
//        => AddProvider<OpenFeatureProviderOptions>(builder, domain, implementationFactory, configureOptions: null);

//    /// <summary>
//    /// Configures policy name options for OpenFeature using the specified options type.
//    /// </summary>
//    /// <typeparam name="TOptions">The type of options used to configure <see cref="OpenFeatureProviderOptions"/>.</typeparam>
//    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
//    /// <param name="configureOptions">A delegate to configure <typeparamref name="TOptions"/>.</param>
//    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
//    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="configureOptions"/> is null.</exception>
//    public static OpenFeatureBuilder AddPolicyName<TOptions>(this OpenFeatureBuilder builder, Action<TOptions> configureOptions)
//        where TOptions : PolicyNameOptions
//    {
//        if (builder == null) throw new ArgumentNullException(nameof(builder));
//        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

//        //builder.IsPolicyConfigured = true;

//        builder.Services.Configure(configureOptions).PostConfigure<OpenFeatureProviderOptions>(options =>
//        {
//            options.IsPolicyConfigured = true;
//        });
//        return builder;
//    }

//    /// <summary>
//    /// Configures the default policy name options for OpenFeature.
//    /// </summary>
//    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
//    /// <param name="configureOptions">A delegate to configure <see cref="OpenFeatureBuilder"/>.</param>
//    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
//    public static OpenFeatureBuilder AddPolicyName(this OpenFeatureProviderBuilder builder, Action<PolicyNameOptions> configureOptions)
//        => AddPolicyName<PolicyNameOptions>(builder, configureOptions);
//}
