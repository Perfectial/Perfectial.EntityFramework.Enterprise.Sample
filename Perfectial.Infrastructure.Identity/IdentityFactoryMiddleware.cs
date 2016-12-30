namespace Perfectial.Infrastructure.Identity
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Owin;

    using Perfectial.Infrastructure.Identity.Base;

    public class IdentityFactoryMiddleware<TResult, TOptions> : OwinMiddleware where TResult : IDisposable where TOptions : IdentityFactoryOptions<TResult>
    {
        private readonly string IdentityKeyPrefix = "Perfectial.Infrastructure.Identity:";

        public TOptions Options { get; private set; }

        public IdentityFactoryMiddleware(OwinMiddleware next, TOptions options)
          : base(next)
        {
            this.Options = options;
        }

        public override async Task Invoke(IOwinContext context)
        {
            TResult instance = this.Options.IdentityFactoryProvider.Create(this.Options, context);

            try
            {
                context.Set(this.GetKey(typeof(TResult)), instance);
                if (this.Next != null)
                {
                    await this.Next.Invoke(context);
                }
            }
            finally
            {
                this.Options.IdentityFactoryProvider.Dispose(this.Options, instance);
            }
        }

        private string GetKey(Type t)
        {
            return this.IdentityKeyPrefix + t.AssemblyQualifiedName;
        }
    }
}
