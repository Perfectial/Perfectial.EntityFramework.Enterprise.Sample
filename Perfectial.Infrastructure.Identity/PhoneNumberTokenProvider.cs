namespace Perfectial.Infrastructure.Identity
{
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    /// <summary>
    /// TokenProvider that generates tokens from the user's security stamp and notifies a user via their phone number.
    /// </summary>
    public class PhoneNumberTokenProvider : TotpSecurityStampBasedTokenProvider
    {
        private readonly IIdentityMessageService identityMessageService;

        public PhoneNumberTokenProvider(IAuthenticationService authenticationService, IIdentityMessageService identityMessageService)
            : base(authenticationService)
        {
            this.identityMessageService = identityMessageService;
        }
        public override Task<bool> IsValidAsync(User user)
        {
            bool isValidProviderForUser = false;

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                isValidProviderForUser = user.PhoneNumberConfirmed;
            }

            return Task.FromResult(isValidProviderForUser);
        }

        public override async Task NotifyAsync(User user, string token)
        {
            var identityMessage = new IdentityMessage
            {
                Subject = string.Empty,
                Body = $"Your security code is {token}",
                Destination = user.PhoneNumber
            };

            await this.identityMessageService.SendAsync(identityMessage);
        }

        protected override Task<string> GetModifierAsync(string modifier, User user)
        {
            var tokenModifier = $"PhoneNumber:{modifier}:{user.PhoneNumber}";

            return Task.FromResult(tokenModifier);
        }
    }
}
