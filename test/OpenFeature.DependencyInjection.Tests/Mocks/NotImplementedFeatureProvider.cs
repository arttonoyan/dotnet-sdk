using OpenFeature.Model;

namespace OpenFeature.DependencyInjection.Tests;

public partial class OpenFeatureBuilderExtensionsTests
{
    public class NotImplementedFeatureProvider : FeatureProvider
    {
        public override Metadata? GetMetadata()
        {
            throw new NotImplementedException();
        }

        public override Task<ResolutionDetails<bool>> ResolveBooleanValueAsync(string flagKey, bool defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ResolutionDetails<double>> ResolveDoubleValueAsync(string flagKey, double defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ResolutionDetails<int>> ResolveIntegerValueAsync(string flagKey, int defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ResolutionDetails<string>> ResolveStringValueAsync(string flagKey, string defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<ResolutionDetails<Value>> ResolveStructureValueAsync(string flagKey, Value defaultValue, EvaluationContext? context = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
