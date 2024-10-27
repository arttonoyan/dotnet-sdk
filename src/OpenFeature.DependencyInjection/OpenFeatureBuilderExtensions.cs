using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenFeature.DependencyInjection;
using OpenFeature.Model;

namespace OpenFeature;

/// <summary>
/// Contains extension methods for the <see cref="OpenFeatureBuilder"/> class.
/// </summary>
public static partial class OpenFeatureBuilderExtensions
{
    /// <summary>
    /// This method is used to add a new context to the service collection.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddContext(
        this OpenFeatureBuilder builder,
        Action<EvaluationContextBuilder> configure)
    {
        Guard.ThrowIfNull(builder);
        Guard.ThrowIfNull(configure);

        return builder.AddContext((b, _) => configure(b));
    }

    /// <summary>
    /// This method is used to add a new context to the service collection.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configure">the desired configuration</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddContext(
        this OpenFeatureBuilder builder,
        Action<EvaluationContextBuilder, IServiceProvider> configure)
    {
        Guard.ThrowIfNull(builder);
        Guard.ThrowIfNull(configure);

        builder.IsContextConfigured = true;
        builder.Services.TryAddTransient(provider =>
        {
            var contextBuilder = EvaluationContext.Builder();
            configure(contextBuilder, provider);
            return contextBuilder.Build();
        });

        return builder;
    }

    /// <summary>
    /// Adds a new feature provider with specified options and configuration builder.
    /// </summary>
    /// <typeparam name="TOptions">The <see cref="OpenFeatureOptions"/> type for configuring the feature provider.</typeparam>
    /// <typeparam name="TProviderBuilder">The type of the provider builder.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureBuilder">The action to configure the provider builder.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddProvider<TOptions, TProviderBuilder>(this OpenFeatureBuilder builder, Action<TProviderBuilder> configureBuilder)
        where TOptions : OpenFeatureOptions, new()
        where TProviderBuilder : FeatureProviderBuilder, new()
    {
        builder.HasDefaultProvider = true;

        builder.Services.Configure<TOptions>(options =>
        {
            options.AddDefaultProviderName();
        });

        builder.Services.AddOptions<TProviderBuilder>()
            .Validate(options => options != null, $"{typeof(TProviderBuilder).Name} configuration is invalid.")
            .Configure(configureBuilder);

        builder.Services.TryAddSingleton(static provider =>
        {
            var providerBuilder = provider.GetRequiredService<IOptions<TProviderBuilder>>().Value;
            return providerBuilder.Build();
        });

        builder.AddClient();

        return builder;
    }

    /// <summary>
    /// Adds a named feature provider with specified options and configuration builder.
    /// </summary>
    /// <typeparam name="TOptions">The <see cref="OpenFeatureOptions"/> type for configuring the feature provider.</typeparam>
    /// <typeparam name="TProviderBuilder">The type of the provider builder.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="name">The unique name of the provider.</param>
    /// <param name="configureBuilder">The action to configure the provider builder.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddProvider<TOptions, TProviderBuilder>(this OpenFeatureBuilder builder, string name, Action<TProviderBuilder> configureBuilder)
        where TOptions : OpenFeatureOptions, new()
        where TProviderBuilder : FeatureProviderBuilder, new()
    {
        Guard.ThrowIfNullOrWhiteSpace(name, nameof(name));

        builder.NamedProviderRegistrationCount++;

        builder.Services.Configure<TOptions>(options =>
        {
            options.AddProviderName(name);
        });

        builder.Services.AddOptions<TProviderBuilder>(name)
            .Validate(options => options != null, $"{typeof(TProviderBuilder).Name} configuration is invalid.")
            .Configure(configureBuilder);

        builder.Services.TryAddKeyedSingleton(name, static (provider, key) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<TProviderBuilder>>();
            var providerBuilder = options.Get(key!.ToString());
            return providerBuilder.Build();
        });

        builder.AddClient(name);

        return builder;
    }

    /// <summary>
    /// Adds a feature client to the service collection, configuring it to work with a specific context if provided.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="name">Optional: The name for the feature client instance.</param>
    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
    internal static OpenFeatureBuilder AddClient(this OpenFeatureBuilder builder, string? name = null)
    {
        if (name == null)
        {
            if (builder.IsContextConfigured)
            {
                builder.Services.TryAddScoped<IFeatureClient>(static provider =>
                {
                    var api = provider.GetRequiredService<Api>();
                    var client = api.GetClient();
                    var context = provider.GetRequiredService<EvaluationContext>();
                    client.SetContext(context);
                    return client;
                });
            }
            else
            {
                builder.Services.TryAddScoped<IFeatureClient>(static provider =>
                {
                    var api = provider.GetRequiredService<Api>();
                    return api.GetClient();
                });
            }
        }
        else
        {
            if (builder.IsContextConfigured)
            {
                builder.Services.TryAddKeyedScoped<IFeatureClient>(name, static (provider, key) =>
                {
                    var api = provider.GetRequiredService<Api>();
                    var client = api.GetClient(key!.ToString());
                    var context = provider.GetRequiredService<EvaluationContext>();
                    client.SetContext(context);
                    return client;
                });
            }
            else
            {
                builder.Services.TryAddKeyedScoped<IFeatureClient>(name, static (provider, key) =>
                {
                    var api = provider.GetRequiredService<Api>();
                    return api.GetClient(key!.ToString());
                });
            }
        }

        return builder;
    }

    /// <summary>
    /// Configures a default client for OpenFeature using the provided factory function.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="clientFactory">
    /// A factory function that creates an <see cref="IFeatureClient"/> based on the service provider and <see cref="PolicyNameOptions"/>.
    /// </param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    internal static OpenFeatureBuilder AddDefaultClient(this OpenFeatureBuilder builder, Func<IServiceProvider, PolicyNameOptions, IFeatureClient> clientFactory)
    {
        builder.Services.AddScoped(provider =>
        {
            var policy = provider.GetRequiredService<IOptions<PolicyNameOptions>>().Value;
            return clientFactory(provider, policy);
        });

        return builder;
    }

    /// <summary>
    /// Configures policy name options for OpenFeature using the specified options type.
    /// </summary>
    /// <typeparam name="TOptions">The type of options used to configure <see cref="PolicyNameOptions"/>.</typeparam>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureOptions">A delegate to configure <typeparamref name="TOptions"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddPolicyName<TOptions>(this OpenFeatureBuilder builder, Action<TOptions> configureOptions)
        where TOptions : PolicyNameOptions
    {
        Guard.ThrowIfNull(builder);
        Guard.ThrowIfNull(configureOptions);

        builder.IsPolicyConfigured = true;

        builder.Services.Configure(configureOptions);
        return builder;
    }

    /// <summary>
    /// Configures the default policy name options for OpenFeature.
    /// </summary>
    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
    /// <param name="configureOptions">A delegate to configure <see cref="PolicyNameOptions"/>.</param>
    /// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
    public static OpenFeatureBuilder AddPolicyName(this OpenFeatureBuilder builder, Action<PolicyNameOptions> configureOptions)
        => AddPolicyName<PolicyNameOptions>(builder, configureOptions);
}
