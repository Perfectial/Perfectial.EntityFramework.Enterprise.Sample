using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using Perfectial.Presentation.Web.Models;

namespace Perfectial.Presentation.Web
{
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;

    using Perfectial.Application.Services;
    using Perfectial.Application.Services.Base;
    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity;
    using Perfectial.Infrastructure.Identity.Model;
    using Perfectial.Infrastructure.Identity.Owin;
    using Perfectial.Infrastructure.Persistence.EntityFramework;

    using SimpleInjector;
    using SimpleInjector.Extensions.ExecutionContextScoping;
    using SimpleInjector.Integration.Web.Mvc;

    using AppBuilderExtensions = Owin.AppBuilderExtensions;

    //using AppBuilderExtensions = Owin.AppBuilderExtensions;

    /*using Perfectial.Infrastructure.Identity;*/

    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();
            container.Register<IUserIdentityApplicationService, UserIdentityApplicationService>(Lifestyle.Scoped);
            container.Verify();

            app.Use(
                async (context, next) =>
                {
                    using (container.BeginExecutionContextScope())
                    {
                        await next();
                    }
                });

            // Configure the db context, user manager and signin manager to use a single instance per request
            AppBuilderExtensions.CreatePerOwinContext(app, () => new ApplicationDbContext());
            //app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            //app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            AppBuilderExtensions.CreatePerOwinContext<ApplicationUserManager>(app, ApplicationUserManager.Create);
            AppBuilderExtensions.CreatePerOwinContext<ApplicationSignInManager>(app, ApplicationSignInManager.Create);

            var securityStampValidator = new SecurityStampValidator(null, null);
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = AuthenticationType.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = securityStampValidator.OnValidateIdentity
                }
            });
            app.UseExternalSignInCookie(AuthenticationType.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(AuthenticationType.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(AuthenticationType.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }
    }
}