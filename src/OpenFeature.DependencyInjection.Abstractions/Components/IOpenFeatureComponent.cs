namespace OpenFeature.DependencyInjection.Abstractions;

/// <summary>
/// Defines a component that participates in OpenFeature DI configuration.
/// </summary>
public interface IOpenFeatureComponent
{
    /// <summary>
    /// Registers services and options into the container.
    /// </summary>
    /// <param name="context">The component context.</param>
    void Register(OpenFeatureComponentContext context);

    /// <summary>
    /// Performs build-time steps after all registrations are complete.
    /// </summary>
    /// <param name="context">The component context.</param>
    void Build(OpenFeatureComponentContext context);
}
