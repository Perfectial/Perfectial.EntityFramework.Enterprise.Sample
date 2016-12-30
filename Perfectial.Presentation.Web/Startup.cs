using Microsoft.Owin;

[assembly: OwinStartup(typeof(Perfectial.Presentation.Web.Startup))]
namespace Perfectial.Presentation.Web
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;

    using AutoMapper;

    using Common.Logging;

    using KatanaContrib.Security.LinkedIn;

    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.DataProtection;
    using Microsoft.Owin.Security.Google;

    using Owin;

    using Perfectial.Application.Services;
    using Perfectial.Application.Services.Base;
    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;
    using Perfectial.Infrastructure.Persistence;
    using Perfectial.Infrastructure.Persistence.Base;
    using Perfectial.Infrastructure.Persistence.EntityFramework;

    using SimpleInjector;
    using SimpleInjector.Extensions.ExecutionContextScoping;
    using SimpleInjector.Integration.Web;

    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var container = this.ConfigureIoCContainer(appBuilder);

            // Enables the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider.
            // Configure the sign in cookie.
            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = AuthenticationType.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.
                    OnValidateIdentity = container.GetInstance<SecurityStampValidator>().OnValidateIdentity
                }
            });

            this.UseExternalSignInCookie(appBuilder, AuthenticationType.ExternalCookie);

            // Enables the application to temporarily store user information 
            // when they are verifying the second factor in the two-factor authentication process.
            this.UseTwoFactorSignInCookie(appBuilder, AuthenticationType.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered 
            // on the device where you logged in from. This is similar to the RememberMe option when you log in.
            this.UseTwoFactorRememberBrowserCookie(appBuilder, AuthenticationType.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //appBuilder.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //appBuilder.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //appBuilder.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            appBuilder.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                ClientId = "1156671639-du87jp277i517j89svd7vf4keropir2l.apps.googleusercontent.com",
                ClientSecret = "j9pAZjmEJiw77583fetu40sa"
            });

            appBuilder.UseLinkedInAuthentication( new LinkedInAuthenticationOptions
            {
                AppId = "86yucvy8r3x0fb",
                AppSecret = "OgXxuBQqrVY3v8kz"
            });
        }

        private Container ConfigureIoCContainer(IAppBuilder appBuilder)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            appBuilder.Use(
                async (context, next) =>
                    {
                        using (container.BeginExecutionContextScope())
                        {
                            await next();
                        }
                    });

            container.Register<ILog>(() => LogManager.GetLogger(typeof(LogManager)));

            container.RegisterCollection<Profile>();
            container.Register<IMapper>(() =>
            {
                Mapper.Initialize(configuration =>
                {
                    var profiles = container.GetAllInstances<Profile>();

                    foreach (var profile in profiles)
                    {
                        configuration.AddProfile(profile);
                    }
                });

                return Mapper.Instance;
            }, Lifestyle.Singleton);

            container.Register<IAmbientDbContextLocator, AmbientDbContextLocator>(Lifestyle.Scoped);
            container.Register<IDbContextScopeFactory>(() => new DbContextScopeFactory());
            container.Register<IUserRepository, UserRepository<ApplicationDbContext>>(Lifestyle.Scoped);

            container.Register<IClaimsIdentityFactory, ClaimsIdentityFactory>(Lifestyle.Scoped);
            container.Register<IPasswordHasher, CryptoPasswordHasher>(Lifestyle.Scoped);
            container.Register<IIdentityValidator<string>>(
                () =>
                    {
                        var passwordValidator = new PasswordValidator
                        {
                            RequiredLength = 6,
                            RequireNonLetterOrDigit = true,
                            RequireDigit = true,
                            RequireLowercase = true,
                            RequireUppercase = true,
                        };

                        return passwordValidator;
                    });
            container.Register<IIdentityValidator<User>>(
                () =>
                    {
                        var userRepository = container.GetInstance<IUserRepository>();
                        var userValidator = new UserValidator(userRepository)
                        {
                            AllowOnlyAlphanumericUserNames = false,
                            RequireUniqueEmail = true
                        };

                        return userValidator;
                    });
            container.Register<IUserTokenProvider>(
                () =>
                    {
                        var dataProtectionProvider = appBuilder.GetDataProtectionProvider();
                        var dataProtector = dataProtectionProvider.Create("ASP.NET Identity");
                        var dataProtectorTokenProvider = new DataProtectorTokenProvider(dataProtector);

                        return dataProtectorTokenProvider;
                    });

            container.Register<IAuthenticationService, Rfc6238AuthenticationService>(Lifestyle.Scoped);
            container.Register<IAuthenticationManager>(
                () =>
                    {
                        if (HttpContext.Current != null && HttpContext.Current.Items["owin.Environment"] == null
                            && container.IsVerifying)
                        {
                            return new OwinContext().Authentication;
                        }

                        return HttpContext.Current.GetOwinContext().Authentication;
                    });
            container.Register<IIdentityProvider, IdentityProvider>(Lifestyle.Scoped);
            container.Register<IIdentityMessageService, SmsService>(Lifestyle.Scoped);
            container.Register<IUserTokenTwoFactorProvider, UserTokenTwoFactorProvider>(Lifestyle.Scoped);

            container.Register<IUserIdentityApplicationService>(
                () =>
                    {
                        IAuthenticationManager authenticationManager = container.GetInstance<IAuthenticationManager>();
                        IClaimsIdentityFactory claimsIdentityFactory = container.GetInstance<IClaimsIdentityFactory>();
                        IPasswordHasher passwordHasher = container.GetInstance<IPasswordHasher>();
                        IIdentityValidator<string> passwordIdentityValidator = container.GetInstance<IIdentityValidator<string>>();
                        IIdentityValidator<User> userIdentityValidator = container.GetInstance<IIdentityValidator<User>>();
                        IUserRepository userRepository = container.GetInstance<IUserRepository>();
                        IUserTokenProvider userTokenProvider = container.GetInstance<IUserTokenProvider>();
                        IIdentityProvider userIdentityProvider = container.GetInstance<IIdentityProvider>();
                        IUserTokenTwoFactorProvider userTokenTwoFactorProvider = container.GetInstance<IUserTokenTwoFactorProvider>();
                        IDbContextScopeFactory dbContextScopeFactory = container.GetInstance<IDbContextScopeFactory>();
                        IMapper mapper = container.GetInstance<IMapper>();
                        ILog logger = container.GetInstance<ILog>();

                        var userIdentityApplicationService = new UserIdentityApplicationService(
                            authenticationManager,
                            claimsIdentityFactory,
                            passwordHasher,
                            passwordIdentityValidator,
                            userIdentityValidator,
                            userRepository,
                            userTokenProvider,
                            userIdentityProvider,
                            userTokenTwoFactorProvider,
                            dbContextScopeFactory,
                            mapper,
                            logger);

                        var authenticationService = container.GetInstance<IAuthenticationService>();
                        userIdentityApplicationService.RegisterTwoFactorAuthenticationProvider("Phone Code", new PhoneNumberTokenProvider(authenticationService, new SmsService()));
                        userIdentityApplicationService.RegisterTwoFactorAuthenticationProvider("Email Code", new EmailTokenProvider(authenticationService, new EmailService()));

                        return userIdentityApplicationService;
                    });

            container.Register<SecurityStampValidator>(
                () =>
                    {
                        IDbContextScopeFactory dbContextScopeFactory = container.GetInstance<IDbContextScopeFactory>();
                        IClaimsIdentityFactory claimsIdentityFactory = container.GetInstance<IClaimsIdentityFactory>();
                        IUserRepository userRepository = container.GetInstance<IUserRepository>();
                        IIdentityProvider identityProvider = container.GetInstance<IIdentityProvider>();

                        var securityStampValidator = new SecurityStampValidator(dbContextScopeFactory, userRepository, claimsIdentityFactory, identityProvider);

                        return securityStampValidator;
                    });

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.Verify();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorDependencyResolver(container);

            return container;
        }

        private void UseExternalSignInCookie(IAppBuilder appBuilder, string authenticationType)
        {
            appBuilder.Properties["Microsoft.Owin.Security.Constants.DefaultSignInAsAuthenticationType"] = authenticationType;

            CookieAuthenticationOptions authenticationOptions = new CookieAuthenticationOptions();
            authenticationOptions.AuthenticationType = authenticationType;
            authenticationOptions.AuthenticationMode = AuthenticationMode.Passive;
            authenticationOptions.CookieName = ".AspNet." + authenticationType;
            authenticationOptions.ExpireTimeSpan = TimeSpan.FromMinutes(5.0);

            appBuilder.UseCookieAuthentication(authenticationOptions);
        }

        private void UseTwoFactorSignInCookie(IAppBuilder appBuilder, string authenticationType, TimeSpan expireTimeSpan)
        {
            CookieAuthenticationOptions authenticationOptions = new CookieAuthenticationOptions();
            authenticationOptions.AuthenticationType = authenticationType;
            authenticationOptions.AuthenticationMode = AuthenticationMode.Passive;
            authenticationOptions.CookieName = ".AspNet." + authenticationType;
            authenticationOptions.ExpireTimeSpan = expireTimeSpan;

            appBuilder.UseCookieAuthentication(authenticationOptions);
        }

        private void UseTwoFactorRememberBrowserCookie(IAppBuilder appBuilder, string authenticationType)
        {
            CookieAuthenticationOptions authenticationOptions = new CookieAuthenticationOptions();
            authenticationOptions.AuthenticationType = authenticationType;
            authenticationOptions.AuthenticationMode = AuthenticationMode.Passive;
            authenticationOptions.CookieName = ".AspNet." + authenticationType;

            appBuilder.UseCookieAuthentication(authenticationOptions);
        }
    }
}