using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Perfectial.Presentation.Web.Startup))]
namespace Perfectial.Presentation.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);
        }
    }
}
