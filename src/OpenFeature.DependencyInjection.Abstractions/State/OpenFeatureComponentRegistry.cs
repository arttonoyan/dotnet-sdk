namespace OpenFeature.DependencyInjection.Abstractions;

/// <summary>
/// Stores component state by type for the current builder instance.
/// </summary>
public sealed class OpenFeatureComponentRegistry
{
    private readonly Dictionary<Type, object> _state = [];

    /// <summary>
    /// Gets existing state or creates it using the provided factory.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="factory">Factory used to create the state when missing.</param>
    /// <returns>The existing or newly created state instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
    public TState GetOrAddState<TState>(Func<TState> factory) where TState : class
    {
        if (factory is null) throw new ArgumentNullException(nameof(factory));

        lock (_state)
        {
            if (_state.TryGetValue(typeof(TState), out var existing))
            {
                return (TState)existing;
            }

            var created = factory();
            _state[typeof(TState)] = created;
            return created;
        }
    }

    /// <summary>
    /// Attempts to retrieve a previously stored state instance.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="value">The state instance when present.</param>
    /// <returns><see langword="true"/> when the state exists.</returns>
    public bool TryGetState<TState>(out TState? value) where TState : class
    {
        lock (_state)
        {
            if (_state.TryGetValue(typeof(TState), out var existing))
            {
                value = (TState)existing;
                return true;
            }
        }

        value = null;
        return false;
    }
}
