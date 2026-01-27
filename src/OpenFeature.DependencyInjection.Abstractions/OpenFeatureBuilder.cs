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
        foreach (var component in _components.OfType<IOpenFeatureBuilderContributor>())
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
}

/// <summary>
/// 
/// </summary>
public interface IOpenFeatureFinalizer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    void Finalize(OpenFeatureComponentContext context);
}

/// <summary>
/// 
/// </summary>
public interface IOpenFeatureBuilderContributor
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    void Build(OpenFeatureComponentContext context);
}

/// <summary>
/// 
/// </summary>
public interface IOpenFeatureValidator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    void Validate(OpenFeatureValidationContext context);
}

/// <summary>
/// 
/// </summary>
public abstract class OpenFeatureComponent : IOpenFeatureComponent, IOpenFeatureFinalizer, IOpenFeatureValidator, IOpenFeatureBuilderContributor
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
    public virtual void Finalize(OpenFeatureComponentContext context) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public virtual void Build(OpenFeatureComponentContext context) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public virtual void Validate(OpenFeatureValidationContext context) { }
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

/// <summary>
/// 
/// </summary>
public sealed class OpenFeatureValidationContext(IServiceCollection services, OpenFeatureComponentRegistory registry)
{
    /// <summary>
    /// 
    /// </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// 
    /// </summary>
    public OpenFeatureComponentRegistory Registry { get; } = registry;


    private readonly List<string> _errors = [];
    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyList<string> Errors => _errors;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void AddError(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Validation message must not be empty.", nameof(message));

        _errors.Add(message);
    }
}

/// <summary>
/// 
/// </summary>
/// <param name="errors"></param>
public sealed class OpenFeatureConfigurationException(IReadOnlyList<string> errors)
    : InvalidOperationException(string.Join(Environment.NewLine, errors))
{
    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyList<string> Errors { get; } = errors;
}


// Version 2
//public sealed class OpenFeatureBuilder(IServiceCollection services)
//{
//    private readonly Dictionary<Type, object> _state = [];

//    /// <summary> The services being configured. </summary>
//    public IServiceCollection Services { get; } = services;

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="component"></param>
//    /// <returns></returns>
//    /// <exception cref="ArgumentNullException"></exception>
//    public OpenFeatureBuilder AddComponent(IOpenFeatureComponent component)
//    {
//        if (component is null) throw new ArgumentNullException(nameof(component));

//        component.Register(new OpenFeatureComponentContext(Services, _state));
//        return this;
//    }
//}

///// <summary>
///// 
///// </summary>
//public sealed class OpenFeatureComponentContext
//{
//    private readonly Dictionary<Type, object> _state;

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="services"></param>
//    /// <param name="state"></param>
//    /// <exception cref="ArgumentNullException"></exception>
//    public OpenFeatureComponentContext(IServiceCollection services, Dictionary<Type, object> state)
//    {
//        Services = services ?? throw new ArgumentNullException(nameof(services));
//        _state = state ?? throw new ArgumentNullException(nameof(state));
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    public IServiceCollection Services { get; }

//    // Type safe state. No domain terms.
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <typeparam name="TState"></typeparam>
//    /// <param name="factory"></param>
//    /// <returns></returns>
//    /// <exception cref="ArgumentNullException"></exception>
//    public TState GetOrAddState<TState>(Func<TState> factory) where TState : class
//    {
//        if (factory is null) throw new ArgumentNullException(nameof(factory));

//        if (_state.TryGetValue(typeof(TState), out var existing))
//            return (TState)existing;

//        var created = factory();
//        _state[typeof(TState)] = created;
//        return created;
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <typeparam name="TState"></typeparam>
//    /// <param name="value"></param>
//    /// <returns></returns>
//    public bool TryGetState<TState>(out TState? value) where TState : class
//    {
//        if (_state.TryGetValue(typeof(TState), out var existing))
//        {
//            value = (TState)existing;
//            return true;
//        }

//        value = null;
//        return false;
//    }
//}

///// <summary>
///// 
///// </summary>
//public interface IOpenFeatureComponent
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="context"></param>
//    void Register(OpenFeatureComponentContext context);
//}

///// <summary>
///// 
///// </summary>
///// <param name="factory"></param>
//internal sealed class DefaultProviderComponent(Func<IServiceProvider, FeatureProvider> factory)
//    : IOpenFeatureComponent
//{
//    public void Register(OpenFeatureComponentContext context)
//    {
//        // bookkeeping entirely owned by Provider.DI
//        var state = context.GetOrAddState(() => new ProviderRegistryState());
//        state.HasDefault = true;

//        // DI registrations
//        context.Services.TryAddTransient(factory);
//    }
//}

//internal sealed class ProviderRegistryState
//{
//    public bool HasDefault { get; set; }
//    public int NamedCount { get; set; }
//    public HashSet<string> Names { get; } = new(StringComparer.Ordinal);
//}
