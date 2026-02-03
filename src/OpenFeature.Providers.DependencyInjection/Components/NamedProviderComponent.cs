using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenFeature.DependencyInjection.Abstractions;
using OpenFeature.Providers.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Registers a provider bound to a specific domain name.
/// </summary>
/// <param name="name">Provider domain name.</param>
/// <param name="factory">Factory for creating the domain-scoped <see cref="FeatureProvider"/>.</param>
internal sealed class NamedProviderComponent(
    string name,
    Func<IServiceProvider, string, FeatureProvider> factory) : OpenFeatureComponent
{
    private readonly string _name = name;
    private readonly Func<IServiceProvider, string, FeatureProvider> _factory = factory;

    public override void Register(OpenFeatureComponentContext context)
    {
        ProviderOptionsRegistration.Ensure(context);
        context.Services.TryAddKeyedTransient(_name, (sp, key) =>
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            return _factory(sp, key.ToString()!);
        });
    }

    public override void Build(OpenFeatureComponentContext context)
    {
        var configuration = context.GetOrAddState(static () => new OpenFeatureProviderConfiguration());
        configuration.AddDomain(_name);
    }
}
