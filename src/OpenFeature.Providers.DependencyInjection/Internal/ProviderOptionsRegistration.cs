using Microsoft.Extensions.DependencyInjection;
using OpenFeature.DependencyInjection.Abstractions;
using OpenFeature.Providers.DependencyInjection;

namespace OpenFeature;

/// <summary>
/// Ensures provider options are registered once and populated from build-time configuration.
/// </summary>
internal static class ProviderOptionsRegistration
{
    public static void Ensure(OpenFeatureComponentContext context)
    {
        var state = context.GetOrAddState(static () => new ProviderOptionsRegistrationState());
        if (state.IsRegistered)
        {
            return;
        }

        state.IsRegistered = true;
        context.Services.AddOptions<OpenFeatureProviderOptions>();

        var configuration = context.GetOrAddState(static () => new OpenFeatureProviderConfiguration());
        context.Services.PostConfigure<OpenFeatureProviderOptions>(configuration.ApplyTo);
    }
}

/// <summary>
/// Tracks one-time registration of provider options.
/// </summary>
internal sealed class ProviderOptionsRegistrationState
{
    public bool IsRegistered { get; set; }
}
