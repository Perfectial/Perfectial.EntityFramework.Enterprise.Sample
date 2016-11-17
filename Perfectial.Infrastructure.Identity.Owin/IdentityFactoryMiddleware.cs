namespace Perfectial.Infrastructure.Identity.Owin
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Owin;

    public class IdentityFactoryMiddleware<TResult, TOptions> : OwinMiddleware where TResult : IDisposable where TOptions : IdentityFactoryOptions<TResult>
    {
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
                context.Set(instance);
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
    }
}
