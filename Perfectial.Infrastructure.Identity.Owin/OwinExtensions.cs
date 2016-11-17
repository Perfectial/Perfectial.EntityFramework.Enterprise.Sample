namespace Perfectial.Infrastructure.Identity.Owin
{
    using System;

    using Microsoft.Owin;

    public static class OwinContextExtensions
    {
        private static readonly string IdentityKeyPrefix = "Perfectial.Infrastructure.Identity.Owin:";

        private static string GetKey(Type t)
        {
            return IdentityKeyPrefix + t.AssemblyQualifiedName;
        }

        public static IOwinContext Set<T>(this IOwinContext context, T value)
        {
            return context.Set(GetKey(typeof(T)), value);
        }

        public static T Get<T>(this IOwinContext context)
        {
            return context.Get<T>(GetKey(typeof(T)));
        }
    }
}