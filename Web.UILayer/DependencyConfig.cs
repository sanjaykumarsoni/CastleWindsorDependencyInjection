using System;
using System.Reflection;
using System.Web.Mvc;
using Castle.Facilities.WcfIntegration;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using System.Diagnostics;
using RequestContext = System.Web.Routing.RequestContext;
using System.Globalization;
using CommonLayer;
using EmployeeService;

namespace Web.UILayer
{
    public class DependencyConfig : DefaultControllerFactory
    {      
        readonly IWindsorContainer _dependencyContainer;

        public DependencyConfig(IWindsorContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
            Type[] controllerTypes = null;
            try
            {
                var webAssembly = Assembly.GetExecutingAssembly();
                try
                {
                    controllerTypes = webAssembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    foreach (var e in ex.LoaderExceptions)
                    {
                        Debug.WriteLine("Get Type Failed to Load" + e.ToString());
                    }
                }
                if (controllerTypes != null)
                {
                    foreach (var t in controllerTypes)
                    {
                        if (typeof(IController).IsAssignableFrom(t))
                        {
                            dependencyContainer.Register(Component.For(t).LifeStyle.Transient);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
                if (controllerType == null) return null;
                return (IController)_dependencyContainer.Resolve(controllerType);
        }

        public override void ReleaseController(IController controller)
        {
            base.ReleaseController(controller);
            _dependencyContainer.Release(controller);
        }

        public static void RegisterDependency()
        {
            ControllerBuilder.Current.SetControllerFactory(new DependencyConfig(DependencyContainer.Container));

            DependencyContainer.Container.AddFacility<WcfFacility>().Register(Component.For<EmployeeService.IEmployeeService>()
               .AsWcfClient(new DefaultClientModel { Endpoint = WcfEndpoint.FromConfiguration("EmployeeWcfServiceEndPoint") })
            );
        }
    }
}