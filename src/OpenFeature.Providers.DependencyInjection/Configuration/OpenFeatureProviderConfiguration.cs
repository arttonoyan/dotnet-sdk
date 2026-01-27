using System.Collections.ObjectModel;

namespace OpenFeature.Providers.DependencyInjection;

/// <summary>
/// Captures provider-related configuration collected during builder setup.
/// </summary>
/// <remarks>
/// This type represents build-time state. The runtime source of truth remains
/// <see cref="OpenFeatureProviderOptions"/>, which is populated from this state
/// during registration. The duplication is intentional to keep build-time logic
/// decoupled from options resolution.
/// </remarks>
public sealed class OpenFeatureProviderConfiguration
{
    private readonly HashSet<string> _domains = new(StringComparer.Ordinal);

    /// <summary>
    /// Determines if a default provider has been registered.
    /// </summary>
    public bool HasDefaultProvider { get; internal set; }

    /// <summary>
    /// Indicates whether a policy name has been configured.
    /// </summary>
    public bool IsPolicyConfigured { get; internal set; }

    /// <summary>
    /// Gets a read-only list of configured provider domains.
    /// </summary>
    public IReadOnlyCollection<string> Domains
    {
        get
        {
            lock (_domains)
            {
                return new ReadOnlyCollection<string>([.. _domains]);
            }
        }
    }

    internal void AddDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
        {
            return;
        }

        lock (_domains)
        {
            _domains.Add(domain);
        }
    }

    /// <summary>
    /// Indicates whether any provider (default or domain-scoped) has been registered.
    /// </summary>
    public bool IsProviderRegistered => HasDefaultProvider || _domains.Count > 0;

    /// <summary>
    /// Validates provider configuration rules that must be enforced at build time.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Validate()
    {
        if (IsPolicyConfigured)
        {
            return;
        }

        if (_domains.Count > 1)
        {
            throw new InvalidOperationException("Multiple providers have been registered, but no policy has been configured.");
        }

        if (HasDefaultProvider && _domains.Count == 1)
        {
            throw new InvalidOperationException("A default provider and an additional provider have been registered without a policy configuration.");
        }
    }
}
