using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.IoC
{
    public class Container : IContainer, IDisposable
    {
        private readonly IDictionary<Type,ComponentDesc> _components = new Dictionary<Type, ComponentDesc>();
        private readonly object _lock = new object();

        public Container()
        {
            RegisterInstance<IContainer>(this);
        }

        public void RegisterAllFromAssembly(Assembly assembly, params Func<Type, bool>[] excludePredicates)
        {
            var allTypes = assembly.GetTypes();
            var allNonAbstractClassesWithPublicConstructor = allTypes.Where(t => t.IsClass && !t.IsAbstract && t.GetConstructors().Length > 0);
            var filteredTypes = excludePredicates.Aggregate(allNonAbstractClassesWithPublicConstructor, (current, excludePredicate) => current.Where(t => !excludePredicate(t)));
            foreach (var filteredType in filteredTypes)
            {
                var interfaces = filteredType.GetInterfaces();
                if (interfaces.Length == 0)
                {
                    DoComponentRegistration(filteredType, filteredType, false);
                }
                foreach (var @interface in interfaces)
                {
                    DoComponentRegistration(@interface, filteredType, false);
                }
            }
        }

        public void RegisterComponent<TService, TImpl>() where TImpl : TService
        {
            DoComponentRegistration(typeof(TService), typeof(TImpl), true);
        }

        public void RegisterComponent(Type serviceType, Type implType)
        {
            DoComponentRegistration(serviceType, implType, true);
        }

        public void RegisterComponent<TService>()
        {
            DoComponentRegistration(typeof(TService), typeof(TService), true);
        }

        public void RegisterComponent(Type serviceType)
        {
            DoComponentRegistration(serviceType, serviceType, true);
        }

        public void RegisterInstance(Type serviceType, object instance)
        {
            DoInstanceRegistration(serviceType, instance, true);
        }

        public void RegisterInstance<TService>(TService instance)
        {
            DoInstanceRegistration(typeof(TService), instance, true);
        }
        
        public void RegisterInstance(object instance)
        {
            DoInstanceRegistration(instance.GetType(), instance, true);
        }

        public T Resolve<T>()
        {
            return (T)DoResolve(typeof (T), true);
        }

        public object Resolve(Type serviceType)
        {
            return DoResolve(serviceType, true);
        }

        public void Install(params IInstaller[] installers)
        {
            foreach (var installer in installers)
            {
                installer.Install(this);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _components.Clear();
            }
        }

        public void InstallFromAllAssemblies()
        {
            var allInstallerTypes = AppDomain.CurrentDomain.GetAssemblies()
                                      .SelectMany(a => a.GetTypes())
                                      .Where(t => typeof (IInstaller).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
            var allInstallers = allInstallerTypes.Select(ActivateInstaller);
            foreach (var installer in allInstallers)
            {
                installer.Install(this);
            }
        }

        private object DoResolve(Type serviceType, bool throwIfNotRegistered)
        {
            lock (_lock)
            {
                if (!_components.ContainsKey(serviceType))
                    if (throwIfNotRegistered)
                        throw new InvalidOperationException(string.Format("Component for service of type {0} is not registered.", serviceType.FullName));
                    else
                        return null;

                var component = _components[serviceType];
                var parameters = component.ConstructorInfo.GetParameters();
                var args = new object[parameters.Length];
                int i = 0;
                foreach (var parameterInfo in parameters)
                {
                    args[i++] = Resolve(parameterInfo.ParameterType);
                }
                component.Instance = component.ConstructorInfo.Invoke(args);
                return component.Instance;
            }
        }

        private void DoComponentRegistration(Type serviceType, Type implType, bool throwIfAlreadyRegistered)
        {
            lock (_lock)
            {
                if (CheckRegistration(serviceType, throwIfAlreadyRegistered)) return;

                var constructorInfo = implType.GetConstructors().FirstOrDefault();
                if (constructorInfo == null)
                    throw new InvalidOperationException(string.Format("Cannot register component of type {0} it has no public constructor.", implType));

                _components.Add(serviceType,
                                new ComponentDesc
                                    {
                                        ConstructorInfo = constructorInfo, 
                                        ImplementationType = implType
                                    });
            }
        }

        private void DoInstanceRegistration(Type serviceType, object instance, bool throwIfAlreadyRegistered)
        {
            lock (_lock)
            {
                if (CheckRegistration(serviceType, throwIfAlreadyRegistered)) return;

                var instanceType = instance.GetType();
                _components.Add(serviceType,
                                new ComponentDesc
                                    {
                                        Instance = instance,
                                        ImplementationType = instanceType
                                    });
            }
        }

        private IInstaller ActivateInstaller(Type installerType)
        {
            return (IInstaller) Activator.CreateInstance(installerType);
        }

        private bool CheckRegistration(Type serviceType, bool throwIfAlreadyRegistered)
        {
            if (_components.ContainsKey(serviceType))
                if (throwIfAlreadyRegistered)
                    throw new InvalidOperationException(string.Format("Component for service of type {0} is already registered.", serviceType.FullName));
                else
                    return true;
            return false;
        }
    }

    public struct ComponentDesc
    {
        public Type ImplementationType { get; set; }
        public object Instance { get; set; }
        public ConstructorInfo ConstructorInfo { get; set; }
    }
}
