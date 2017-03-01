using System;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.WcfIntegration;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Proxy;
using Castle.Core;
using System.Globalization;
using System.IO;
using System.Web.Configuration;

namespace CommonLayer
{
    /// <summary>
    /// Windsor Registrar to register all the assemblies on PerWebRequest
    /// </summary>
    public sealed class DependencyContainer
    {
        private static readonly object LockObject = new object();

        private static IWindsorContainer _container;

        private static DependencyContainer _instance;

        private DependencyContainer() { }

        public static IWindsorContainer Container
        {
            get
            {
                if (_container == null)
                {
                    lock (LockObject)
                    {
                        _container = new WindsorContainer();
                        String[] dependentAssembiles =
                            ConfigurationManager.AppSettings["DependentAssemblies"].Split(';');
                        foreach (var dependentAssembile in dependentAssembiles)
                        {
                            if (!String.IsNullOrWhiteSpace(dependentAssembile))
                            {
                                RegisterAllClassesFromAssembly(dependentAssembile);
                            }
                        }

                        #region AOP Related
                       _container.Kernel.ProxyFactory.AddInterceptorSelector(new InterceptorsSettings());
                        // Register(typeof(InstrumentationInterceptor));
                        Register(typeof(LoggingInterceptor));
                        // Register(typeof(SecurityInterceptor));
                           
                        #endregion AOP Related
                    }
                }
                return _container;
            }
            set
            {
                lock (LockObject)
                {
                    _container = value;
                }
            }
        }

        public static DependencyContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new DependencyContainer();
                        }
                    }
                }

                return _instance;
            }
        }

        public static T GetInstance<T>()
        {
            var obj = Container.Resolve(typeof(T));
            return (T)obj;
        }

        public static object GetInstance(Type type)
        {
            return Container.Resolve(type);
        }

        public static void ReleaseInstance(object instance)
        {
            Container.Release(instance);
        }

        public static void Register(Type implementationType)
        {
            Container.Register(Component.For(implementationType));
        }

        public static void Register(Type interfaceType, Type implementationType)
        {
            Container.Register(
            Component.For(interfaceType).ImplementedBy(implementationType).LifestyleTransient());
            Container.Register(
                   Component.For(interfaceType).ImplementedBy(implementationType).LifestyleTransient());
            Container.Register(
                   Component.For(interfaceType).ImplementedBy(implementationType).LifestyleSingleton());
            Container.Register(
                   Component.For(interfaceType).ImplementedBy(implementationType).LifestyleSingleton());
            Container.Register(
                    Component.For(interfaceType).ImplementedBy(implementationType).LifestylePerWebRequest());
        }

        public static void RegisterAllClassesFromAssembly(String a)
        {
            Container.Register(AllTypes.FromAssemblyNamed(a).Pick().WithService.AllInterfaces().LifestylePerWcfOperation());
        }
    }

    public class InterceptorsSettings : IModelInterceptorsSelector
    {
        public Boolean HasInterceptors(Castle.Core.ComponentModel model)
        {
            String[] interceptClasses = ConfigurationManager.AppSettings["InterceptClasses"].Split(';');
            foreach (var interceptClass in interceptClasses)
            {
                if (!String.IsNullOrWhiteSpace(interceptClass))
                {
                    if (model.Name.EndsWith(interceptClass))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public InterceptorReference[] SelectInterceptors(Castle.Core.ComponentModel model, Castle.Core.InterceptorReference[] interceptors)
        {
            var intercept = new List<InterceptorReference>();
            String enableInstrumentation = ConfigurationManager.AppSettings["EnableInstrumentation"];
            if (String.IsNullOrEmpty(enableInstrumentation) != true &&
                Convert.ToBoolean(enableInstrumentation, CultureInfo.InvariantCulture))
            {
                intercept.Add(InterceptorReference.ForType<LoggingInterceptor>());
            }
            return intercept.Concat(interceptors).ToArray();
        }
    }
}
