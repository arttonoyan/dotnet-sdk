using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature.DependencyInjection.Abstractions;

/// <summary>
/// Provides access to the service collection and component state during configuration.
/// </summary>
public sealed class OpenFeatureComponentContext
{
    private readonly OpenFeatureComponentRegistry _registry;

    internal OpenFeatureComponentContext(IServiceCollection services, OpenFeatureComponentRegistry registry)
    {
        Services = services;
        _registry = registry;
    }

    /// <summary>
    /// The service collection being configured.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Get or create component state scoped to the builder.
    /// </summary>
    /// <param name="factory">Factory used to create the state when missing.</param>
    public TState GetOrAddState<TState>(Func<TState> factory) where TState : class
        => _registry.GetOrAddState(factory);

    /// <summary>
    /// Try to get component state scoped to the builder.
    /// </summary>
    /// <param name="value">The state instance when present.</param>
    /// <returns><see langword="true"/> when the state exists.</returns>
    public bool TryGetState<TState>(out TState? value) where TState : class
        => _registry.TryGetState(out value);
}
