using System;

namespace RIMAPI.Core
{
    public interface IServiceCollection
    {
        void AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;
        void AddSingleton<TService>(TService instance)
            where TService : class;
        void AddSingleton<TService>()
            where TService : class;
        void AddSingleton<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class;

        // ADD THESE OVERLOADS:
        void AddSingleton(Type serviceType, Type implementationType);
        void AddSingleton(Type serviceType, object instance);
        void AddSingleton(Type serviceType, Func<IServiceProvider, object> implementationFactory);

        void AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;
        void AddTransient<TService>()
            where TService : class;
        void AddTransient<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class;

        // ADD THESE OVERLOADS:
        void AddTransient(Type serviceType, Type implementationType);
        void AddTransient(Type serviceType, Func<IServiceProvider, object> implementationFactory);

        IServiceProvider BuildServiceProvider();
    }
}
