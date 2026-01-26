//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Extensions;
//using OpenFeature.Model;

//namespace OpenFeature.DependencyInjection.Abstractions;

///// <summary>
///// Extension methods for configuring the hosted feature lifecycle in the <see cref="OpenFeatureBuilder"/>.
///// </summary>
//public static partial class OpenFeatureBuilderExtensions
//{
//    /// <summary>
//    /// This method is used to add a new context to the service collection.
//    /// </summary>
//    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
//    /// <param name="configure">the desired configuration</param>
//    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
//    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="configure"/> action is null.</exception>
//    public static OpenFeatureBuilder AddContext(this OpenFeatureBuilder builder, Action<EvaluationContextBuilder> configure)
//    {
//        Guard.ThrowIfNull(builder);
//        Guard.ThrowIfNull(configure);

//        return builder.AddContext((b, _) => configure(b));
//    }

//    /// <summary>
//    /// This method is used to add a new context to the service collection.
//    /// </summary>
//    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
//    /// <param name="configure">the desired configuration</param>
//    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
//    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or <paramref name="configure"/> action is null.</exception>
//    public static OpenFeatureBuilder AddContext(this OpenFeatureBuilder builder, Action<EvaluationContextBuilder, IServiceProvider> configure)
//    {
//        Guard.ThrowIfNull(builder);
//        Guard.ThrowIfNull(configure);

//        builder.IsContextConfigured = true;
//        builder.Services.TryAddTransient(provider =>
//        {
//            var contextBuilder = EvaluationContext.Builder();
//            configure(contextBuilder, provider);
//            return contextBuilder.Build();
//        });

//        return builder;
//    }

//    /// <summary>
//    /// Adds a feature client to the service collection, configuring it to work with a specific context if provided.
//    /// </summary>
//    /// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
//    /// <param name="name">Optional: The name for the feature client instance.</param>
//    /// <returns>The <see cref="OpenFeatureBuilder"/> instance.</returns>
//    internal static OpenFeatureBuilder AddClient(this OpenFeatureBuilder builder, string? name = null)
//    {
//        if (string.IsNullOrWhiteSpace(name))
//        {
//            builder.Services.TryAddScoped<IFeatureClient>(static provider =>
//            {
//                var api = provider.GetRequiredService<Api>();
//                var client = api.GetClient();

//                var context = provider.GetService<EvaluationContext>();
//                if (context is not null)
//                {
//                    client.SetContext(context);
//                }

//                return client;
//            });
//        }
//        else
//        {
//            builder.Services.TryAddKeyedScoped<IFeatureClient>(name, static (provider, key) =>
//            {
//                var api = provider.GetRequiredService<Api>();
//                var client = api.GetClient(key!.ToString());

//                var context = provider.GetService<EvaluationContext>();
//                if (context is not null)
//                {
//                    client.SetContext(context);
//                }

//                return client;
//            });
//        }

//        return builder;
//    }

//    ///// <summary>
//    ///// Adds a default <see cref="IFeatureClient"/> to the <see cref="OpenFeatureBuilder"/> based on the policy name options.
//    ///// This method configures the dependency injection container to resolve the appropriate <see cref="IFeatureClient"/>
//    ///// depending on the policy name selected.
//    ///// If no name is selected (i.e., null), it retrieves the default client.
//    ///// </summary>
//    ///// <param name="builder">The <see cref="OpenFeatureBuilder"/> instance.</param>
//    ///// <returns>The configured <see cref="OpenFeatureBuilder"/> instance.</returns>
//    //internal static OpenFeatureBuilder AddPolicyBasedClient(this OpenFeatureBuilder builder)
//    //{
//    //    builder.Services.AddScoped(provider =>
//    //    {
//    //        var policy = provider.GetRequiredService<IOptions<PolicyNameOptions>>().Value;
//    //        var name = policy.DefaultNameSelector(provider);
//    //        return ResolveFeatureClient(provider, name);
//    //    });

//    //    return builder;
//    //}

//    //private static IFeatureClient ResolveFeatureClient(IServiceProvider provider, string? name = null)
//    //{
//    //    var api = provider.GetRequiredService<Api>();
//    //    var client = api.GetClient(name);
//    //    var context = provider.GetService<EvaluationContext>();
//    //    if (context != null)
//    //    {
//    //        client.SetContext(context);
//    //    }

//    //    return client;
//    //}
//}
