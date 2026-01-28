using Microsoft.Extensions.DependencyInjection;
using OpenFeature.DependencyInjection.Abstractions;
using OpenFeature.Providers.DependencyInjection;

namespace OpenFeature.Hosting.Tests;

public class OpenFeatureBuilderTests
{
    [Fact]
    public void Validate_DoesNotThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);
        var config = builder.Build().GetProviderConfiguration();

        // Act
        var ex = Record.Exception(config.Validate);

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_WithPolicySet_DoesNotThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services)
            .AddPolicyName(_ => { });
        var config = builder.Build().GetProviderConfiguration();

        // Act
        var ex = Record.Exception(config.Validate);

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_WithMultipleDomainProvidersRegistered_ThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services)
            .AddProvider("domain-a", (_, _) => new NoOpFeatureProvider())
            .AddProvider("domain-b", (_, _) => new NoOpFeatureProvider());
        var config = builder.Build().GetProviderConfiguration();

        // Act
        var ex = Assert.Throws<InvalidOperationException>(config.Validate);

        // Assert
        Assert.Equal("Multiple providers have been registered, but no policy has been configured.", ex.Message);
    }

    [Fact]
    public void Validate_WithDefaultAndDomainProvidersRegistered_ThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services)
            .AddProvider(_ => new NoOpFeatureProvider())
            .AddProvider("domain-a", (_, _) => new NoOpFeatureProvider());
        var config = builder.Build().GetProviderConfiguration();

        // Act
        var ex = Assert.Throws<InvalidOperationException>(config.Validate);

        // Assert
        Assert.Equal("A default provider and an additional provider have been registered without a policy configuration.", ex.Message);
    }

    [Fact]
    public void Validate_WithNoDefaultProviderRegistered_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services)
            .AddProvider("domain-a", (_, _) => new NoOpFeatureProvider());
        var config = builder.Build().GetProviderConfiguration();

        // Act
        var ex = Record.Exception(config.Validate);

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_WithPolicyAndMultipleProviders_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services)
            .AddProvider(_ => new NoOpFeatureProvider())
            .AddProvider("domain-a", (_, _) => new NoOpFeatureProvider())
            .AddPolicyName(_ => { });
        var config = builder.Build().GetProviderConfiguration();

        // Act
        var ex = Record.Exception(config.Validate);

        // Assert
        Assert.Null(ex);
    }
}
