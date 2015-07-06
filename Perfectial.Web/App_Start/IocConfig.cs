using Autofac;
using Autofac.Integration.Mvc;
using Perfectial.Infrastructure.Services.Interfaces;
using Perfectial.Infrastructure.Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Perfectial.Web.App_Start
{
    public class IocConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();

            var currentAssembly = Assembly.GetCallingAssembly();
            builder.RegisterControllers(currentAssembly);

            //services
            builder.RegisterType(typeof(UserService)).As(typeof(IUserService)).InstancePerRequest();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        public static TAbstraction ResolveInstance<TAbstraction>()
        {
            return DependencyResolver.Current.GetService<TAbstraction>();
        }
    }
}