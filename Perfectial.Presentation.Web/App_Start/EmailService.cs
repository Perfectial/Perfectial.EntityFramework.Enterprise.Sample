namespace Perfectial.Presentation.Web
{
    using System.Threading.Tasks;

    using Microsoft.AspNet.Identity;

    using Perfectial.Infrastructure.Identity;

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // TODO: Plug in your email service here to send an email.

            return Task.FromResult(0);
        }
    }
}