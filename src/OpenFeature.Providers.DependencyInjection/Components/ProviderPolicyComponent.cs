using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenFeature.DependencyInjection.Abstractions;
using OpenFeature.Providers.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Registers policy name options and marks policy configuration as present.
/// </summary>
internal sealed class ProviderPolicyComponent : OpenFeatureComponent
{
    public override void Register(OpenFeatureComponentContext context)
    {
        context.Services.AddOptions<PolicyNameOptions>();
        context.Services.PostConfigure<PolicyNameOptions>(options =>
        {
            if (options.DefaultNameSelector is not null)
            {
                return;
            }

            options.DefaultNameSelector = sp =>
            {
                var providerOptions = sp.GetRequiredService<IOptions<OpenFeatureProviderOptions>>().Value;
                return providerOptions.ProviderNames.FirstOrDefault();
            };
        });
    }

    public override void Build(OpenFeatureComponentContext context)
    {
        var configuration = context.GetOrAddState(static () => new OpenFeatureProviderConfiguration());
        configuration.IsPolicyConfigured = true;
    }
}
