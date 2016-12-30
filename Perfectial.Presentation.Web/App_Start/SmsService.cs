namespace Perfectial.Presentation.Web
{
    using System.Threading.Tasks;

    using Perfectial.Infrastructure.Identity;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.

            return Task.FromResult(0);
        }
    }
}