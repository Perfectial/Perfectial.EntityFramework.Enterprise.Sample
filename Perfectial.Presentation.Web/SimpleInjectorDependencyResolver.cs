namespace Perfectial.Presentation.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.Dependencies;
    using System.Web.Mvc;

    using SimpleInjector;

    public class SimpleInjectorDependencyResolver : System.Web.Mvc.IDependencyResolver, System.Web.Http.Dependencies.IDependencyResolver
    {
        public SimpleInjectorDependencyResolver(Container container)
        {
            this.Container = container;
        }

        public Container Container { get; }

        public object GetService(Type serviceType)
        {
            return this.GetServiceInternal(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.GetServicesInternal(serviceType);
        }

        IDependencyScope System.Web.Http.Dependencies.IDependencyResolver.BeginScope()
        {
            return this;
        }

        object IDependencyScope.GetService(Type serviceType)
        {
            return this.GetServiceInternal(serviceType);
        }

        IEnumerable<object> IDependencyScope.GetServices(Type serviceType)
        {
            return this.GetServicesInternal(serviceType);
        }

        void IDisposable.Dispose()
        {
        }

        private object GetServiceInternal(Type serviceType)
        {
            object service;
            if (!serviceType.IsAbstract && typeof(IController).IsAssignableFrom(serviceType))
            {
                service = this.Container.GetInstance(serviceType);
            }
            else
            {
                service = ((IServiceProvider)this.Container).GetService(serviceType);
            }

            return service;
        }

        private IEnumerable<object> GetServicesInternal(Type serviceType)
        {
            var collectionType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var services = (IEnumerable<object>)((IServiceProvider)this.Container).GetService(collectionType);

            return services ?? Enumerable.Empty<object>();
        }
    }
}