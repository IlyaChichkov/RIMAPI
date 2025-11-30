using System;
using System.Collections.Generic;
using System.Linq;

namespace RIMAPI.Core
{
    public class ServiceCollection : IServiceCollection
    {
        private readonly Dictionary<Type, ServiceDescriptor> _descriptors =
            new Dictionary<Type, ServiceDescriptor>();

        public void AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _descriptors[typeof(TService)] = new ServiceDescriptor(
                typeof(TService),
                typeof(TImplementation),
                ServiceLifetime.Singleton
            );
        }

        public void AddSingleton(Type serviceType, Type implementationType)
        {
            _descriptors[serviceType] = new ServiceDescriptor(
                serviceType,
                implementationType,
                ServiceLifetime.Singleton
            );
        }

        public void AddSingleton(Type serviceType, object instance)
        {
            _descriptors[serviceType] = new ServiceDescriptor(serviceType, instance);
        }

        public void AddSingleton(
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            _descriptors[serviceType] = new ServiceDescriptor(
                serviceType,
                implementationFactory,
                ServiceLifetime.Singleton
            );
        }

        public void AddTransient(Type serviceType, Type implementationType)
        {
            _descriptors[serviceType] = new ServiceDescriptor(
                serviceType,
                implementationType,
                ServiceLifetime.Transient
            );
        }

        public void AddTransient(
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            _descriptors[serviceType] = new ServiceDescriptor(
                serviceType,
                implementationFactory,
                ServiceLifetime.Transient
            );
        }

        public void AddSingleton<TService>(TService instance)
            where TService : class
        {
            _descriptors[typeof(TService)] = new ServiceDescriptor(typeof(TService), instance);
        }

        public void AddSingleton<TService>()
            where TService : class
        {
            _descriptors[typeof(TService)] = new ServiceDescriptor(
                typeof(TService),
                typeof(TService),
                ServiceLifetime.Singleton
            );
        }

        public void AddSingleton<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            _descriptors[typeof(TService)] = new ServiceDescriptor(
                typeof(TService),
                implementationFactory,
                ServiceLifetime.Singleton
            );
        }

        public void AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _descriptors[typeof(TService)] = new ServiceDescriptor(
                typeof(TService),
                typeof(TImplementation),
                ServiceLifetime.Transient
            );
        }

        public void AddTransient<TService>()
            where TService : class
        {
            _descriptors[typeof(TService)] = new ServiceDescriptor(
                typeof(TService),
                typeof(TService),
                ServiceLifetime.Transient
            );
        }

        public void AddTransient<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            _descriptors[typeof(TService)] = new ServiceDescriptor(
                typeof(TService),
                implementationFactory,
                ServiceLifetime.Transient
            );
        }

        public IServiceProvider BuildServiceProvider()
        {
            return new ServiceProvider(_descriptors);
        }
    }

    public enum ServiceLifetime
    {
        Singleton,
        Transient,
    }

    public class ServiceDescriptor
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public object Implementation { get; set; }
        public Func<IServiceProvider, object> ImplementationFactory { get; }
        public ServiceLifetime Lifetime { get; }

        public ServiceDescriptor(
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifetime
        )
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Lifetime = lifetime;
        }

        public ServiceDescriptor(Type serviceType, object implementation)
        {
            ServiceType = serviceType;
            Implementation = implementation;
            Lifetime = ServiceLifetime.Singleton;
        }

        public ServiceDescriptor(
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory,
            ServiceLifetime lifetime
        )
        {
            ServiceType = serviceType;
            ImplementationFactory = implementationFactory;
            Lifetime = lifetime;
        }
    }

    public class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, ServiceDescriptor> _descriptors;
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        public ServiceProvider(Dictionary<Type, ServiceDescriptor> descriptors)
        {
            _descriptors = descriptors;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public object GetService(Type serviceType)
        {
            if (!_descriptors.TryGetValue(serviceType, out var descriptor))
            {
                throw new InvalidOperationException(
                    $"Service of type {serviceType.Name} is not registered"
                );
            }

            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                // Check if we have a factory method
                if (descriptor.ImplementationFactory != null)
                {
                    if (_singletons.TryGetValue(serviceType, out var factorySingleton))
                        return factorySingleton;

                    var factoryInstance = descriptor.ImplementationFactory(this); // Renamed to factoryInstance
                    _singletons[serviceType] = factoryInstance;
                    return factoryInstance;
                }

                // Check if we have a concrete instance
                if (descriptor.Implementation != null)
                    return descriptor.Implementation;

                // Check if we already created this singleton
                if (_singletons.TryGetValue(serviceType, out var singleton))
                    return singleton;

                // Create new instance
                var newInstance = CreateInstance(descriptor.ImplementationType); // Renamed to newInstance
                _singletons[serviceType] = newInstance;
                return newInstance;
            }
            else // Transient
            {
                // Check if we have a factory method
                if (descriptor.ImplementationFactory != null)
                {
                    return descriptor.ImplementationFactory(this);
                }

                // Create new instance
                return CreateInstance(descriptor.ImplementationType);
            }
        }

        private object CreateInstance(Type implementationType)
        {
            var constructor = implementationType.GetConstructors().First();
            var parameters = constructor.GetParameters();

            if (parameters.Length == 0)
                return Activator.CreateInstance(implementationType);

            var parameterInstances = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterInstances[i] = GetService(parameters[i].ParameterType);
            }

            return constructor.Invoke(parameterInstances);
        }
    }
}
