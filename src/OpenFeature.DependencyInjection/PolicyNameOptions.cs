namespace OpenFeature;

/// <summary>
/// Options to configure the default feature client name selection for OpenFeature.
/// </summary>
public class PolicyNameOptions
{
    /// <summary>
    /// A delegate to select the default feature client name based on the service provider context.
    /// </summary>
    public Func<IServiceProvider, string?>? DefaultNameSelector { get; set; }
}
