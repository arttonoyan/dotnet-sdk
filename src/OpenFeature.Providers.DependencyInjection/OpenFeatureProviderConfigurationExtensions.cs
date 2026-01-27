using OpenFeature.DependencyInjection.Abstractions;

namespace OpenFeature.Providers.DependencyInjection;

/// <summary>
/// Helper extensions for provider configuration state.
/// </summary>
public static class OpenFeatureProviderConfigurationExtensions
{
    /// <summary>
    /// Gets provider configuration from component state, creating it if missing.
    /// </summary>
    /// <param name="state">The component state.</param>
    /// <returns>The provider configuration.</returns>
    public static OpenFeatureProviderConfiguration GetProviderConfiguration(this OpenFeatureComponentRegistory state)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));

        return state.GetOrAddState(static () => new OpenFeatureProviderConfiguration());
    }
}
