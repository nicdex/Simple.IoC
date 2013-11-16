using System;
using System.Reflection;

namespace Simple.IoC
{
    public interface IContainer
    {
        void RegisterAllFromAssembly(Assembly assembly, params Func<Type, bool>[] excludePredicates);
        void RegisterComponent<TService, TImpl>() where TImpl : TService;
        void RegisterComponent(Type serviceType, Type implType);
        void RegisterComponent<TService>();
        void RegisterComponent(Type serviceType);
        void RegisterInstance(Type serviceType, object instance);
        void RegisterInstance<TService>(TService instance);
        void RegisterInstance(object instance);
        
        T Resolve<T>();
        object Resolve(Type serviceType);

        void Install(params IInstaller[] installers);
        void InstallFromAllAssemblies();
    }
}