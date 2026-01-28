namespace OpenFeature.DependencyInjection.Abstractions;

/// <summary>
/// Base type for OpenFeature components with default no-op build behavior.
/// </summary>
public abstract class OpenFeatureComponent : IOpenFeatureComponent
{
    /// <summary>
    /// Registers services and options into the container.
    /// </summary>
    /// <param name="context">The component context.</param>
    public abstract void Register(OpenFeatureComponentContext context);

    /// <summary>
    /// Performs build-time steps after all registrations are complete.
    /// </summary>
    /// <param name="context">The component context.</param>
    public virtual void Build(OpenFeatureComponentContext context) { }
}
