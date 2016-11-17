namespace Perfectial.Infrastructure.Identity.Owin
{
    using System;

    using global::Owin;

    using Microsoft.Owin;
    using Microsoft.Owin.Security.DataProtection;

    public static class AppBuilderExtensions
    {
        public static IAppBuilder CreatePerOwinContext<T>(this IAppBuilder app, Func<T> createCallback) where T : class, IDisposable
        {
            return app.CreatePerOwinContext<T>((options, context) => createCallback());
        }

        public static IAppBuilder CreatePerOwinContext<T>(this IAppBuilder app, Func<IdentityFactoryOptions<T>, IOwinContext, T> createCallback) where T : class, IDisposable
        {
            return app.CreatePerOwinContext<T>(createCallback, (options, instance) => instance.Dispose());
        }

        public static IAppBuilder CreatePerOwinContext<T>(this IAppBuilder app, Func<IdentityFactoryOptions<T>, IOwinContext, T> createCallback, Action<IdentityFactoryOptions<T>, T> disposeCallback) where T : class, IDisposable
        {
            app.Use(typeof(IdentityFactoryMiddleware<T, IdentityFactoryOptions<T>>), new IdentityFactoryOptions<T>
                   {
                       DataProtectionProvider = app.GetDataProtectionProvider(),
                       IdentityFactoryProvider = new IdentityFactoryProvider<T>
                                      {
                                          OnCreate = createCallback,
                                          OnDispose = disposeCallback
                                      }
                   });

            return app;
        }

        // TODO: Add other extension methods when necessary.
    }
}
