namespace Perfectial.Presentation.Web
{
    using System.Web.Http;

    public static class WebApiConfiguration
    {
        public static void Register(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.MapHttpAttributeRoutes();

            httpConfiguration.Routes.MapHttpRoute(
                name: "DefaultApi", 
                routeTemplate: "api/{controller}/{id}", 
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
