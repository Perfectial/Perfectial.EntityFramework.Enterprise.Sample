namespace Perfectial.Infrastructure.Identity.Owin
{
    using System;

    using Microsoft.Owin;

    public interface IIdentityFactoryProvider<T> where T : IDisposable
    {
        T Create(IdentityFactoryOptions<T> options, IOwinContext context);
        void Dispose(IdentityFactoryOptions<T> options, T instance);
    }
}
