namespace Perfectial.Infrastructure.Identity
{
    using System;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    public class Rfc6238AuthenticationService: IAuthenticationService
    {
        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan timeStep = TimeSpan.FromMinutes(3.0);
        private static readonly Encoding encoding = new UTF8Encoding(false, true);

        public int GenerateCode(SecurityToken securityToken, string modifier = null)
        {
            ulong currentTimeStepNumber = this.GetCurrentTimeStepNumber();
            using (HMACSHA1 hmacshA1 = new HMACSHA1(securityToken.GetDataNoClone()))
            {
                return this.ComputeTotp(hmacshA1, currentTimeStepNumber, modifier);
            }
        }

        public bool ValidateCode(SecurityToken securityToken, int code, string modifier = null)
        {
            var codeIsValid = false;

            ulong currentTimeStepNumber = this.GetCurrentTimeStepNumber();
            using (HMACSHA1 hmacshA1 = new HMACSHA1(securityToken.GetDataNoClone()))
            {
                for (int index = -2; index <= 2; ++index)
                {
                    if (this.ComputeTotp(hmacshA1, currentTimeStepNumber + (ulong)index, modifier) == code)
                    {
                        codeIsValid = true;
                        break;
                    }
                }
            }

            return codeIsValid;
        }

        private ulong GetCurrentTimeStepNumber()
        {
            var currentTimeStepNumber = (ulong)((DateTime.UtcNow - unixEpoch).Ticks / timeStep.Ticks);

            return currentTimeStepNumber;
        }

        private int ComputeTotp(HashAlgorithm hashAlgorithm, ulong timeStepNumber, string modifier)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)timeStepNumber));
            byte[] hash = hashAlgorithm.ComputeHash(this.ApplyModifier(bytes, modifier));
            int index = hash[hash.Length - 1] & 15;
            int totp = ((hash[index] & sbyte.MaxValue) << 24 | (hash[index + 1] & byte.MaxValue) << 16 | (hash[index + 2] & byte.MaxValue) << 8 | hash[index + 3] & byte.MaxValue) % 1000000;

            return totp;
        }

        private byte[] ApplyModifier(byte[] input, string modifier)
        {
            if (string.IsNullOrEmpty(modifier))
            {
                return input;
            }

            byte[] bytes = encoding.GetBytes(modifier);
            byte[] modifiedInput = new byte[checked(input.Length + bytes.Length)];
            Buffer.BlockCopy(input, 0, modifiedInput, 0, input.Length);
            Buffer.BlockCopy(bytes, 0, modifiedInput, input.Length, bytes.Length);

            return modifiedInput;
        }
    }
}