using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OpenFeature.Model;
using Xunit;

namespace OpenFeature.DependencyInjection.Tests;

public partial class OpenFeatureBuilderExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddContext_Delegate_ShouldAddServiceToCollection(bool useServiceProviderDelegate)
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act
        var result = useServiceProviderDelegate ?
            builder.AddContext(_ => { }) :
            builder.AddContext((_, _) => { });

        // Assert
        result.Should().BeSameAs(builder, "The method should return the same builder instance.");
        services.Should().ContainSingle(serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(EvaluationContext) &&
            serviceDescriptor.Lifetime == ServiceLifetime.Transient,
            "A transient service of type EvaluationContext should be added.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddContext_Delegate_ShouldCorrectlyHandles(bool useServiceProviderDelegate)
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);
        bool delegateCalled = false;

        _ = useServiceProviderDelegate ?
            builder.AddContext(_ => delegateCalled = true) :
            builder.AddContext((_, _) => delegateCalled = true);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var context = serviceProvider.GetService<EvaluationContext>();

        // Assert
        context.Should().NotBeNull("The EvaluationContext should be resolvable.");
        delegateCalled.Should().BeTrue("The delegate should be invoked.");
    }

    [Fact]
    public void AddProvider_ShouldAddProviderToCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);

        // Act
        var result = builder.AddProvider(_ => new NoOpFeatureProvider());

        // Assert
        result.Should().BeSameAs(builder, "The method should return the same builder instance.");
        services.Should().ContainSingle(serviceDescriptor =>
            serviceDescriptor.ServiceType == typeof(FeatureProvider) &&
            serviceDescriptor.Lifetime == ServiceLifetime.Singleton,
            "A singleton service of type FeatureProvider should be added.");
    }

    [Fact]
    public void AddProvider_ShouldResolveCorrectProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new OpenFeatureBuilder(services);
        builder.AddProvider(_ => new NoOpFeatureProvider());

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var provider = serviceProvider.GetService<FeatureProvider>();

        // Assert
        provider.Should().NotBeNull("The FeatureProvider should be resolvable.");
        provider.Should().BeOfType<NoOpFeatureProvider>("The resolved provider should be of type DefaultFeatureProvider.");
    }
}
