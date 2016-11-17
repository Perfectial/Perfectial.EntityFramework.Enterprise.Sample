namespace Perfectial.Infrastructure.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;

    public class UserTokenTwoFactorProvider : IUserTokenTwoFactorProvider
    {
        private readonly Dictionary<string, IUserTokenProvider> userTokenTwoFactorProviders = new Dictionary<string, IUserTokenProvider>(); 
        public Task RegisterTwoFactorUserTokenProviderAsync(string twoFactorProviderKey, IUserTokenProvider userTokenProvider)
        {
            this.userTokenTwoFactorProviders[twoFactorProviderKey] = userTokenProvider;

            return Task.FromResult(0);
        }

        public async Task<IList<string>> GetValidTwoFactorUserTokenProvidersAsync(User user)
        {
            var validTwoFactorUserTokenProviders = new List<string>();
            foreach (var userTokenTwoFactorProvider in this.userTokenTwoFactorProviders)
            {
                if (await userTokenTwoFactorProvider.Value.IsValidAsync(user))
                {
                    validTwoFactorUserTokenProviders.Add(userTokenTwoFactorProvider.Key);
                }
            }

            return validTwoFactorUserTokenProviders;
        }

        public async Task<string> GenerateTwoFactorUserTokenAsync(string twoFactorProviderKey, User user)
        {
            if (!this.userTokenTwoFactorProviders.ContainsKey(twoFactorProviderKey))
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resource.NoTwoFactorProvider, twoFactorProviderKey ));
            }

            IUserTokenProvider userTokenProvider = this.userTokenTwoFactorProviders[twoFactorProviderKey];
            var twoFactorUserToken = await userTokenProvider.GenerateAsync(twoFactorProviderKey, user);

            return twoFactorUserToken;
        }

        public async Task<bool> ValidateTwoFactorUserTokenAsync(string twoFactorProviderKey, User user, string token)
        {
            if (!this.userTokenTwoFactorProviders.ContainsKey(twoFactorProviderKey))
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resource.NoTwoFactorProvider, twoFactorProviderKey));
            }

            IUserTokenProvider userTokenProvider = this.userTokenTwoFactorProviders[twoFactorProviderKey];
            bool twoFactorUserTokenIsValid = await userTokenProvider.ValidateAsync(twoFactorProviderKey, user, token);

            return twoFactorUserTokenIsValid;
        }

        public async Task NotifyTwoFactorUserTokenAsync(string twoFactorProviderKey, User user, string token)
        {
            if (!this.userTokenTwoFactorProviders.ContainsKey(twoFactorProviderKey))
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resource.NoTwoFactorProvider, twoFactorProviderKey));
            }

            IUserTokenProvider userTokenProvider = this.userTokenTwoFactorProviders[twoFactorProviderKey];
            await userTokenProvider.NotifyAsync(user, token);
        }
    }
}