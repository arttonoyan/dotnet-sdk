using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature.DependencyInjection.Abstractions;

/// <summary>
/// 
/// </summary>
public sealed class OpenFeatureBuilder
{
    private readonly OpenFeatureComponentRegistory _registory;
    private readonly List<IOpenFeatureComponent> _components;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public OpenFeatureBuilder(IServiceCollection services)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        _registory = new OpenFeatureComponentRegistory();
        _components = [];
        Services = services;
    }

    internal IReadOnlyList<IOpenFeatureComponent> Components => _components;
    internal OpenFeatureComponentRegistory Registry => _registory;

    /// <summary>
    /// 
    /// </summary>
    public bool IsContextConfigured { get; internal set; }

    /// <summary>The service collection being configured.</summary>
    public IServiceCollection Services { get; }

    /// <summary>Registers a component into the OpenFeature configuration.</summary>
    public OpenFeatureBuilder AddComponent(IOpenFeatureComponent component)
    {
        if (component is null) throw new ArgumentNullException(nameof(component));

        _components.Add(component);
        component.Register(new OpenFeatureComponentContext(Services, _registory));
        return this;
    }

    /// <summary>
    /// Runs all build steps and returns the component state.
    /// </summary>
    public OpenFeatureComponentRegistory Build()
    {
        var context = CreateComponentContext();
        foreach (var component in _components)
        {
            component.Build(context);
        }

        return _registory;
    }

    internal OpenFeatureComponentContext CreateComponentContext() => new(Services, _registory);
}

/// <summary>
/// 
/// </summary>
public sealed class OpenFeatureComponentContext
{
    private readonly OpenFeatureComponentRegistory _registry;

    internal OpenFeatureComponentContext(IServiceCollection services, OpenFeatureComponentRegistory registry)
    {
        Services = services;
        _registry = registry;
    }

    /// <summary>
    /// 
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Get or create component state scoped to the builder.
    /// </summary>
    public TState GetOrAddState<TState>(Func<TState> factory) where TState : class
        => _registry.GetOrAddState(factory);

    /// <summary>
    /// Try to get component state scoped to the builder.
    /// </summary>
    public bool TryGetState<TState>(out TState? value) where TState : class
        => _registry.TryGetState(out value);
}

/// <summary>
/// 
/// </summary>
public interface IOpenFeatureComponent
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    void Register(OpenFeatureComponentContext context);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    void Build(OpenFeatureComponentContext context);
}

/// <summary>
/// 
/// </summary>
/// <summary>
/// 
/// </summary>
public abstract class OpenFeatureComponent : IOpenFeatureComponent
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public abstract void Register(OpenFeatureComponentContext context);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public virtual void Build(OpenFeatureComponentContext context) { }
}

/// <summary>
/// 
/// </summary>
public sealed class OpenFeatureComponentRegistory
{
    private readonly Dictionary<Type, object> _state = new();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="factory"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
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
    /// 
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
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
