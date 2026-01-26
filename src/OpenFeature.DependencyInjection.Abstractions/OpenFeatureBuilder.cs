using Microsoft.Extensions.DependencyInjection;

namespace OpenFeature.DependencyInjection.Abstractions;

///// <summary>
///// Describes a <see cref="OpenFeatureBuilder"/> backed by an <see cref="IServiceCollection"/>.
///// </summary>
///// <param name="services">The services being configured.</param>
//public class OpenFeatureBuilder(IServiceCollection services)
//{
//    /// <summary> The services being configured. </summary>
//    public IServiceCollection Services { get; } = services;

//    /// <summary>
//    /// Indicates whether the evaluation context has been configured.
//    /// This property is used to determine if specific configurations or services
//    /// should be initialized based on the presence of an evaluation context.
//    /// </summary>
//    public bool IsContextConfigured { get; internal set; }
//}

// Version 3

/// <summary>
/// 
/// </summary>
public sealed class OpenFeatureBuilder
{
    private readonly Dictionary<Type, object> _state;
    private readonly List<IOpenFeatureComponent> _components;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public OpenFeatureBuilder(IServiceCollection services)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        _state = [];
        _components = [];
        Services = services;
    }

    internal IReadOnlyList<IOpenFeatureComponent> Components => _components;

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
        component.Register(new OpenFeatureComponentContext(Services, _state));
        return this;
    }

    internal OpenFeatureComponentContext CreateContext() => new(Services, _state);
}

/// <summary>
/// 
/// </summary>
public sealed class OpenFeatureComponentContext
{
    private readonly Dictionary<Type, object> _state;

    internal OpenFeatureComponentContext(IServiceCollection services, Dictionary<Type, object> state)
    {
        Services = services;
        _state = state;
    }

    /// <summary>
    /// 
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>Gets an existing state instance of <typeparamref name="TState"/> or creates and stores one.</summary>
    public TState GetOrAddState<TState>(Func<TState> factory) where TState : class
    {
        if (factory is null) throw new ArgumentNullException(nameof(factory));

        if (_state.TryGetValue(typeof(TState), out var existing))
        {
            return (TState)existing;
        }

        var created = factory();
        _state[typeof(TState)] = created;
        return created;
    }

    /// <summary>Gets an existing state instance of <typeparamref name="TState"/> if present.</summary>
    public bool TryGetState<TState>(out TState? value) where TState : class
    {
        if (_state.TryGetValue(typeof(TState), out var existing))
        {
            value = (TState)existing;
            return true;
        }

        value = null;
        return false;
    }
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
/// <param name="services"></param>
public sealed class OpenFeatureValidationContext(IServiceProvider services)
{
    /// <summary>
    /// 
    /// </summary>
    public IServiceProvider Services { get; } = services;


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
