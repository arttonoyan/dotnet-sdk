using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature.DependencyInjection.Abstractions;

/// <summary>
/// Coordinates OpenFeature DI configuration and component build steps.
/// </summary>
public sealed class OpenFeatureBuilder
{
    private readonly OpenFeatureComponentRegistry _registory;
    private readonly List<IOpenFeatureComponent> _components;

    /// <summary>
    /// Initializes a new builder for the provided service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public OpenFeatureBuilder(IServiceCollection services)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        _registory = new();
        _components = [];
        Services = services;
    }

    internal IReadOnlyList<IOpenFeatureComponent> Components => _components;
    internal OpenFeatureComponentRegistry Registry => _registory;

    /// <summary>
    /// Indicates whether an evaluation context has been configured.
    /// </summary>
    public bool IsContextConfigured { get; internal set; }

    /// <summary>The service collection being configured.</summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Registers a component into the OpenFeature configuration.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <returns>The current builder.</returns>
    public OpenFeatureBuilder AddComponent(IOpenFeatureComponent component)
    {
        if (component is null) throw new ArgumentNullException(nameof(component));

        _components.Add(component);
        component.Register(new OpenFeatureComponentContext(Services, _registory));
        return this;
    }

    /// <summary>
    /// Runs all build steps and returns the component registry state.
    /// </summary>
    public OpenFeatureComponentRegistry Build()
    {
        var context = CreateComponentContext();
        foreach (IOpenFeatureComponent component in _components)
        {
            component.Build(context);
        }

        return _registory;
    }

    internal OpenFeatureComponentContext CreateComponentContext() => new(Services, _registory);
}
