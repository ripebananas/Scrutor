using System;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor;

internal static class ServiceDescriptorExtensions
{
    public static ServiceDescriptor WithImplementationFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object?, object> implementationFactory, ServiceLifetime serviceLifetime) =>
        new(descriptor.ServiceType, descriptor.ServiceKey, implementationFactory, serviceLifetime);

    public static ServiceDescriptor WithServiceKey(this ServiceDescriptor descriptor, string serviceKey, ServiceLifetime serviceLifetime) =>
        descriptor.IsKeyedService ? ReplaceServiceKey(descriptor, serviceKey, serviceLifetime) : AddServiceKey(descriptor, serviceKey, serviceLifetime);

    private static ServiceDescriptor ReplaceServiceKey(ServiceDescriptor descriptor, string serviceKey, ServiceLifetime serviceLifetime) => descriptor switch
    {
        { KeyedImplementationType: not null } => new ServiceDescriptor(descriptor.ServiceType, serviceKey, descriptor.KeyedImplementationType, serviceLifetime),
        { KeyedImplementationFactory: not null } => new ServiceDescriptor(descriptor.ServiceType, serviceKey, descriptor.KeyedImplementationFactory, serviceLifetime),
        { KeyedImplementationInstance: not null } => new ServiceDescriptor(descriptor.ServiceType, serviceKey, descriptor.KeyedImplementationInstance),
        _ => throw new ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor))
    };

    private static ServiceDescriptor AddServiceKey(ServiceDescriptor descriptor, string serviceKey, ServiceLifetime serviceLifetime) => descriptor switch
    {
        { ImplementationType: not null } => new ServiceDescriptor(descriptor.ServiceType, serviceKey, descriptor.ImplementationType, serviceLifetime),
        { ImplementationFactory: not null } => new ServiceDescriptor(descriptor.ServiceType, serviceKey, DiscardServiceKey(descriptor.ImplementationFactory), serviceLifetime),
        { ImplementationInstance: not null } => new ServiceDescriptor(descriptor.ServiceType, serviceKey, descriptor.ImplementationInstance),
        _ => throw new ArgumentException($"No implementation factory or instance or type found for {descriptor.ServiceType}.", nameof(descriptor))
    };

    private static Func<IServiceProvider, object?, object> DiscardServiceKey(Func<IServiceProvider, object> factory) => (sp, key) => factory(sp);
}
