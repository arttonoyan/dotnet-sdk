namespace OpenFeature.DependencyInjection.Tests;

public class NoOpFeatureProviderFactory : IFeatureProviderFactory
{
    public FeatureProvider Create() => new NoOpFeatureProvider();
}
