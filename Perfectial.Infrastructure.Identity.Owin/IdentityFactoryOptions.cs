namespace Perfectial.Infrastructure.Identity.Owin
{
    using System;

    using Microsoft.Owin.Security.DataProtection;

    public class IdentityFactoryOptions<T> where T : IDisposable
    {
        public IDataProtectionProvider DataProtectionProvider { get; set; }
        public IIdentityFactoryProvider<T> IdentityFactoryProvider { get; set; }
    }
}
