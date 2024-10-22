using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Describes a <see cref="OpenFeatureBuilder"/> backed by an <see cref="IServiceCollection"/>.
/// </summary>
/// <param name="services">The services being configured.</param>
public class OpenFeatureBuilder(IServiceCollection services)
{
    /// <summary> The services being configured. </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Indicates whether the evaluation context has been configured.
    /// This property is used to determine if specific configurations or services
    /// should be initialized based on the presence of an evaluation context.
    /// </summary>
    public bool IsContextConfigured { get; internal set; }
}
