namespace Perfectial.Presentation.Web
{
    using System.Web.Mvc;

    public class ModelBindersConfiguration
    {
        public static void Register()
        {
            ModelBinders.Binders.DefaultBinder = new DependencyModelBinder();
        }
    }
}
