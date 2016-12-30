namespace Perfectial.Presentation.Web
{
    using System;
    using System.Web.Mvc;

    public class DependencyModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            return DependencyResolver.Current.GetService(modelType);
        }
    }
}