namespace Perfectial.Infrastructure.Identity.Owin
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security.DataProtection;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;

    /// <summary>
    /// Token provider that uses an IDataProtector to generate encrypted tokens based off of the security stamp.
    /// </summary>
    public class DataProtectorTokenProvider : IUserTokenProvider
    {
        private readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

        private readonly IDataProtector dataProtector;
        public TimeSpan TokenLifespan { get; set; } = TimeSpan.FromDays(1.0);

        public DataProtectorTokenProvider(IDataProtector dataProtector)
        {
            this.dataProtector = dataProtector;
        }

        public Task<string> GenerateAsync(string modifier, User user)
        {
            string securityToken;

            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream, this.DefaultEncoding, true))
                {
                    binaryWriter.Write(DateTimeOffset.UtcNow.UtcTicks);
                    binaryWriter.Write(Convert.ToString(user.Id, CultureInfo.InvariantCulture));
                    binaryWriter.Write(modifier ?? string.Empty);
                    binaryWriter.Write(user.SecurityStamp ?? string.Empty);
                }

                byte[] protectedBytes = this.dataProtector.Protect(memoryStream.ToArray());
                securityToken = Convert.ToBase64String(protectedBytes);
            }

            return Task.FromResult(securityToken);
        }

        public Task<bool> ValidateAsync(string modifier, User user, string token)
        {
            bool tokenIsValid;
            try
            {
                byte[] unprotectedData = this.dataProtector.Unprotect(Convert.FromBase64String(token));

                var memoryStream = new MemoryStream(unprotectedData);
                using (var binaryReader = new BinaryReader(memoryStream, this.DefaultEncoding, true))
                {
                    DateTimeOffset creationTime = new DateTimeOffset(binaryReader.ReadInt64(), TimeSpan.Zero);
                    DateTimeOffset expirationTime = creationTime + this.TokenLifespan;

                    if (expirationTime < DateTimeOffset.UtcNow)
                    {
                        tokenIsValid = false;
                    }
                    else
                    {
                        string userId = binaryReader.ReadString();
                        if (!string.Equals(userId, Convert.ToString(user.Id, CultureInfo.InvariantCulture)))
                        {
                            tokenIsValid = false;
                        }
                        else
                        {
                            string expectedModifier = binaryReader.ReadString();
                            if (!string.Equals(expectedModifier, modifier))
                            {
                                tokenIsValid = false;
                            }
                            else
                            {
                                string securityStamp = binaryReader.ReadString();
                                if (binaryReader.PeekChar() != -1)
                                {
                                    tokenIsValid = false;
                                }
                                else
                                {
                                    tokenIsValid = securityStamp == user.SecurityStamp;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                tokenIsValid = false;
            }

            return Task.FromResult(tokenIsValid);
        }

        public Task<bool> IsValidAsync(User user)
        {
            return Task.FromResult(true);
        }

        public Task NotifyAsync(User user, string token)
        {
            return Task.FromResult(0);
        }
    }
}
