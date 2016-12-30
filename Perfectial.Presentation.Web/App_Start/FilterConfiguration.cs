namespace Perfectial.Presentation.Web
{
    using System.Web.Mvc;

    public class FilterConfiguration
    {
        public static void Register(GlobalFilterCollection globalFilters)
        {
            globalFilters.Add(new HandleErrorAttribute());
            globalFilters.Add(new AuthorizeAttribute());
            globalFilters.Add(new RequireHttpsAttribute());
        }
    }
}
