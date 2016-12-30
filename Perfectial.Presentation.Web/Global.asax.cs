namespace Perfectial.Presentation.Web
{
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfiguration.Register);

            ModelBindersConfiguration.Register();
            FilterConfiguration.Register(GlobalFilters.Filters);
            RouteConfiguration.Register(RouteTable.Routes);
            BundleConfiguration.Register(BundleTable.Bundles);
        }
    }
}