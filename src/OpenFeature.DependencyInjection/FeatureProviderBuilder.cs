namespace OpenFeature;

/// <summary>
/// Represents an abstract base class for building feature providers.
/// This builder provides the blueprint for constructing specific <see cref="FeatureProvider"/> instances.
/// </summary>
public abstract class FeatureProviderBuilder
{
    /// <summary>
    /// Constructs and returns an instance of a <see cref="FeatureProvider"/> configured according to the implementation.
    /// This method should be implemented by derived classes to return a fully configured <see cref="FeatureProvider"/> instance.
    /// </summary>
    /// <returns>An instance of <see cref="FeatureProvider"/>.</returns>
    public abstract FeatureProvider Build();
}
