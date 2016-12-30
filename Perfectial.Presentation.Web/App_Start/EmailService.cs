namespace Perfectial.Presentation.Web
{
    using System.Threading.Tasks;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // TODO: Plug in your email service here to send an email.

            return Task.FromResult(0);
        }
    }
}