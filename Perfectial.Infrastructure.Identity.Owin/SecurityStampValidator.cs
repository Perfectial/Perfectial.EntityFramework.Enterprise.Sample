namespace Perfectial.Infrastructure.Identity.Owin
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security.Cookies;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    public class SecurityStampValidator
    {
        private static readonly TimeSpan DefaultValidationInterval = TimeSpan.FromMinutes(30);

        private readonly IUserRepository userRepository;
        private readonly IClaimsIdentityFactory claimsIdentityFactory;
        public SecurityStampValidator(IUserRepository userRepository, IClaimsIdentityFactory claimsIdentityFactory)
        {
            this.userRepository = userRepository;
            this.claimsIdentityFactory = claimsIdentityFactory;
        }

        public TimeSpan ValidationInterval { get; set; } = DefaultValidationInterval;

        public async Task OnValidateIdentity(CookieValidateIdentityContext cookieValidateIdentityContext)
        {
            DateTimeOffset currentDateUtc = DateTimeOffset.UtcNow;
            if (cookieValidateIdentityContext.Options?.SystemClock != null)
            {
                currentDateUtc = cookieValidateIdentityContext.Options.SystemClock.UtcNow;
            }

            DateTimeOffset? authenticationTickedIssuedDataUtc = cookieValidateIdentityContext.Properties.IssuedUtc;
            bool authenticationTickedIsValid = !authenticationTickedIssuedDataUtc.HasValue;
            if (authenticationTickedIssuedDataUtc.HasValue)
            {
                authenticationTickedIsValid = currentDateUtc.Subtract(authenticationTickedIssuedDataUtc.Value) > this.ValidationInterval;
            }

            if (authenticationTickedIsValid)
            {
                var userId = cookieValidateIdentityContext.Identity.GetUserId();
                if (userId != null)
                {
                    bool userIdentityIsValid = false;

                    User user = await this.userRepository.FindByIdAsync(userId);
                    if (user != null)
                    {
                        string securityStamp = cookieValidateIdentityContext.Identity.FindFirst("AspNet.Identity.SecurityStamp")?.Value;
                        if (securityStamp == user.SecurityStamp)
                        {
                            var userRoles = await this.userRepository.GetRolesAsync(user);
                            var userClaims = await this.userRepository.GetClaimsAsync(user);
                            var claimsIdentity = await this.claimsIdentityFactory.CreateAsync(user, AuthenticationType.ApplicationCookie, userRoles, userClaims);
                            // Add custom user claims here.

                            cookieValidateIdentityContext.Properties.IssuedUtc = new DateTimeOffset?();
                            cookieValidateIdentityContext.Properties.ExpiresUtc = new DateTimeOffset?();
                            cookieValidateIdentityContext.OwinContext.Authentication.SignIn(cookieValidateIdentityContext.Properties, claimsIdentity);

                            userIdentityIsValid = true;
                        }
                    }

                    if (!userIdentityIsValid)
                    {
                        cookieValidateIdentityContext.RejectIdentity();
                        cookieValidateIdentityContext.OwinContext.Authentication.SignOut(cookieValidateIdentityContext.Options.AuthenticationType);
                    }
                }
            }
        }
    }
}
