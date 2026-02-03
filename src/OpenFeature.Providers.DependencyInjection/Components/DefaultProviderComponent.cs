using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.DependencyInjection.Abstractions;
using OpenFeature.Providers.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Registers the default provider and tracks that a default provider is configured.
/// </summary>
/// <param name="factory">Factory for creating the default <see cref="FeatureProvider"/>.</param>
internal sealed class DefaultProviderComponent(Func<IServiceProvider, FeatureProvider> factory) : OpenFeatureComponent
{
    private readonly Func<IServiceProvider, FeatureProvider> _factory = factory;

    public override void Register(OpenFeatureComponentContext context)
    {
        ProviderOptionsRegistration.Ensure(context);
        context.Services.TryAddTransient(_factory);
    }

    public override void Build(OpenFeatureComponentContext context)
    {
        var configuration = context.GetOrAddState(static () => new OpenFeatureProviderConfiguration());
        configuration.HasDefaultProvider = true;
    }
}
