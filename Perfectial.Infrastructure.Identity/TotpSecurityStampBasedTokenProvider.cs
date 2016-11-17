namespace Perfectial.Infrastructure.Identity
{
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    /// <summary>
    /// TokenProvider that generates time based codes using the user's security stamp.
    /// </summary>
    public class TotpSecurityStampBasedTokenProvider : IUserTokenProvider
    {
        private readonly IAuthenticationService authenticationService;

        public TotpSecurityStampBasedTokenProvider(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        public virtual async Task<string> GenerateAsync(string modifier, User user)
        {
            SecurityToken token = this.CreateSecurityTokenAsync(user);
            string securityTokenModifier = await this.GetModifierAsync(modifier, user);

            return this.authenticationService.GenerateCode(token, securityTokenModifier).ToString("D6", CultureInfo.InvariantCulture);
        }

        public virtual async Task<bool> ValidateAsync(string modifier, User user, string token)
        {
            bool tokenIsValid;

            int code;
            if (!int.TryParse(token, out code))
            {
                tokenIsValid = false;
            }
            else
            {
                SecurityToken securityToken = this.CreateSecurityTokenAsync(user);
                string securityTokenModifier = await this.GetModifierAsync(modifier, user);

                tokenIsValid = securityToken != null && this.authenticationService.ValidateCode(securityToken, code, securityTokenModifier);
            }

            return tokenIsValid;
        }

        public virtual Task NotifyAsync(User user, string token)
        {
            return Task.FromResult(0);
        }

        public virtual Task<bool> IsValidAsync(User user)
        {
            // TODO: Add support for provider verification by extracting interface.

            return Task.FromResult(true);
        }

        protected virtual Task<string> GetModifierAsync(string modifier, User user)
        {
            var tokenModifier = string.Concat("Totp:", modifier, ":", user.Id);

            return Task.FromResult(tokenModifier);
        }

        private SecurityToken CreateSecurityTokenAsync(User user)
        {
            var securityToken = new SecurityToken(Encoding.Unicode.GetBytes(user.SecurityStamp));

            return securityToken;
        }
    }
}
