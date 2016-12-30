namespace Perfectial.Presentation.Web.Controllers
{
    using System.Web.Mvc;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Presentation.Web.Models;

    public class HomeController : Controller
    {
        private readonly IIdentityProvider userIdentityProvider;

        public HomeController(IIdentityProvider userIdentityProvider)
        {
            this.userIdentityProvider = userIdentityProvider;
        }

        public ActionResult Index()
        {
            var homeIndexViewModel = new HomeIndexViewModel(this.userIdentityProvider);

            return this.View(homeIndexViewModel);
        }
    }
}