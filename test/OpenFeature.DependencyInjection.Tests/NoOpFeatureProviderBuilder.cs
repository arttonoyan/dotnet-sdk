namespace OpenFeature.DependencyInjection.Tests;

public class NoOpFeatureProviderBuilder : FeatureProviderBuilder
{
    public override FeatureProvider Build() => new NoOpFeatureProvider();
}
