using System.Collections.ObjectModel;
using OpenFeature.DependencyInjection.Abstractions;

namespace OpenFeature.Providers.DependencyInjection;

/// <summary>
/// Provider-focused options for configuring OpenFeature integrations.
/// Contains only contracts and metadata that integrations may need.
/// </summary>
public class OpenFeatureProviderOptions : OpenFeatureOptions
{
    private readonly HashSet<string> _providerNames = [];

    /// <summary>
    /// Determines if a default provider has been registered.
    /// </summary>
    public bool HasDefaultProvider { get; internal set; }

    /// <summary>
    /// Gets the count of domain-bound providers that have been registered.
    /// This count does not include the default provider.
    /// </summary>
    public int DomainBoundProviderRegistrationCount { get; internal set; }

    /// <summary>
    /// Indicates whether the policy has been configured.
    /// </summary>
    [Obsolete("This property is no longer used.")]
    public bool IsPolicyConfigured { get; internal set; }

    /// <summary>
    /// The <see cref="Type"/> of the configured feature provider, if any.
    /// Typically set by higher-level configuration.
    /// </summary>
    public Type FeatureProviderType { get; protected internal set; } = null!;

    /// <summary>
    /// Gets a read-only list of registered provider names.
    /// </summary>
    public IReadOnlyCollection<string> ProviderNames
    {
        get
        {
            lock (_providerNames)
            {
                return new ReadOnlyCollection<string>([.. _providerNames]);
            }
        }
    }

    /// <summary>
    /// Registers the default provider name if no specific name is provided.
    /// Sets <see cref="HasDefaultProvider"/> to true.
    /// </summary>
    internal void AddDefaultProviderName() => AddProviderName(null);

    /// <summary>
    /// Registers a new feature provider name. This operation is thread-safe.
    /// </summary>
    /// <param name="name">The name of the feature provider to register. Registers as default if null.</param>
    internal void AddProviderName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            HasDefaultProvider = true;
            return;
        }

        lock (_providerNames)
        {
            _providerNames.Add(name!);
        }
    }

    ///// <summary>
    ///// Validates the current configuration, ensuring that a policy is set when multiple providers are registered
    ///// or when a default provider is registered alongside another provider.
    ///// </summary>
    ///// <exception cref="InvalidOperationException">
    ///// Thrown if multiple providers are registered without a policy, or if both a default provider 
    ///// and an additional provider are registered without a policy configuration.
    ///// </exception>
    //public void Validate()
    //{
    //    if (IsPolicyConfigured)
    //    {
    //        return;
    //    }

    //    if (DomainBoundProviderRegistrationCount > 1)
    //    {
    //        throw new InvalidOperationException("Multiple providers have been registered, but no policy has been configured.");
    //    }

    //    if (HasDefaultProvider && DomainBoundProviderRegistrationCount == 1)
    //    {
    //        throw new InvalidOperationException("A default provider and an additional provider have been registered without a policy configuration.");
    //    }
    //}
}
