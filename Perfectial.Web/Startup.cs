using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Perfectial.Web.Startup))]
namespace Perfectial.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
