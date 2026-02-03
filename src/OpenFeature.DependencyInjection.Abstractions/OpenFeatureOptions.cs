namespace OpenFeature.DependencyInjection.Abstractions;

/// <summary>
/// Options used to configure OpenFeature integration.
/// </summary>
public class OpenFeatureOptions
{
    private readonly HashSet<string> _hookNames = [];

    /// <summary>
    /// The configured hook names.
    /// </summary>
    internal IReadOnlyCollection<string> HookNames => _hookNames;

    /// <summary>
    /// Adds a hook name to the configured list.
    /// </summary>
    /// <param name="name">The hook name to register.</param>
    internal void AddHookName(string name)
    {
        lock (_hookNames)
        {
            _hookNames.Add(name);
        }
    }
}
