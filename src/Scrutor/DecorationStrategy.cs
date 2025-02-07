using System;
using Microsoft.Extensions.DependencyInjection;

namespace Scrutor;

public abstract class DecorationStrategy
{
    protected DecorationStrategy(Type serviceType, string? serviceKey, ServiceLifetime? decoratorLifetime)
    {
        ServiceType = serviceType;
        ServiceKey = serviceKey;
        DecoratorLifetime = decoratorLifetime;
    }

    public Type ServiceType { get; }

    public string? ServiceKey { get; }

    public ServiceLifetime? DecoratorLifetime { get; }

    public virtual bool CanDecorate(ServiceDescriptor descriptor) =>
        // object.Equals is used to support decorating services with object keys (e.g., KeyedService.AnyKey).
        Equals(ServiceKey, descriptor.ServiceKey) && CanDecorate(descriptor.ServiceType);

    protected abstract bool CanDecorate(Type serviceType);

    public abstract Func<IServiceProvider, object?, object> CreateDecorator(Type serviceType, string serviceKey);

    internal static DecorationStrategy WithType(Type serviceType, string? serviceKey, Type decoratorType) =>
        Create(serviceType, serviceKey, decoratorType, decoratorFactory: null, decoratorLifetime: null);

    internal static DecorationStrategy WithType(Type serviceType, string? serviceKey, Type decoratorType, ServiceLifetime? decoratorLifetime) =>
        Create(serviceType, serviceKey, decoratorType, decoratorFactory: null, decoratorLifetime);

    internal static DecorationStrategy WithFactory(Type serviceType, string? serviceKey, Func<object, IServiceProvider, object> decoratorFactory) =>
        Create(serviceType, serviceKey, decoratorType: null, decoratorFactory, decoratorLifetime: null);

    internal static DecorationStrategy WithFactory(Type serviceType, string? serviceKey, Func<object, IServiceProvider, object> decoratorFactory, ServiceLifetime? decoratorLifetime) =>
        Create(serviceType, serviceKey, decoratorType: null, decoratorFactory, decoratorLifetime);

    protected static Func<IServiceProvider, object?, object> TypeDecorator(Type serviceType, string serviceKey, Type decoratorType)
    {
        return (serviceProvider, _) =>
        {
            var instanceToDecorate = serviceProvider.GetRequiredKeyedService(serviceType, serviceKey);
            return ActivatorUtilities.CreateInstance(serviceProvider, decoratorType, instanceToDecorate);
        };
    }

    protected static Func<IServiceProvider, object?, object> FactoryDecorator(Type serviceType, string serviceKey, Func<object, IServiceProvider, object> decoratorFactory) => (serviceProvider, _) =>
    {
        var instanceToDecorate = serviceProvider.GetRequiredKeyedService(serviceType, serviceKey);
        return decoratorFactory(instanceToDecorate, serviceProvider);
    };

    private static DecorationStrategy Create(Type serviceType, string? serviceKey, Type? decoratorType, Func<object, IServiceProvider, object>? decoratorFactory, ServiceLifetime? decoratorLifetime)
    {
        if (serviceType.IsOpenGeneric())
        {
            return new OpenGenericDecorationStrategy(serviceType, serviceKey, decoratorType, decoratorFactory, decoratorLifetime);
        }

        return new ClosedTypeDecorationStrategy(serviceType, serviceKey, decoratorType, decoratorFactory, decoratorLifetime);
    }
}
