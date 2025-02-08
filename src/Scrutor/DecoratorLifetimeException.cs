using System;

namespace Scrutor;

public class DecoratorLifetimeException : InvalidOperationException
{
    public DecoratorLifetimeException(DecorationStrategy strategy)
        : base($"Decorator of service '{strategy.ServiceType.ToFriendlyName()}' has a longer lifetime than the decorated service.")
    {
        Strategy = strategy;
    }

    public DecorationStrategy Strategy { get; }
}
