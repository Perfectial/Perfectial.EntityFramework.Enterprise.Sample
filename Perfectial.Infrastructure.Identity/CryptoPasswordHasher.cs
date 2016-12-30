
namespace Perfectial.Infrastructure.Identity
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;

    using Perfectial.Infrastructure.Identity.Base;

    public class CryptoPasswordHasher : IPasswordHasher
    {
        private const int PBKDF2IterationCount = 1000;
        private const int PBKDF2SubkeyLength = 32;
        private const int SaltSize = 16;

        public virtual string HashPassword(string password)
        {
            byte[] salt;
            byte[] bytes;

            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, 16, 1000))
            {
                salt = rfc2898DeriveBytes.Salt;
                bytes = rfc2898DeriveBytes.GetBytes(32);
            }

            byte[] inArray = new byte[49];
            Buffer.BlockCopy(salt, 0, inArray, 1, 16);
            Buffer.BlockCopy(bytes, 0, inArray, 17, 32);

            return Convert.ToBase64String(inArray);
        }

        public virtual bool VerifyHashedPassword(string passwordHash, string password)
        {
            byte[] hashedPasswordInBytes = Convert.FromBase64String(passwordHash);
            if (hashedPasswordInBytes.Length != 49 || hashedPasswordInBytes[0] != 0)
            {
                return false;
            }

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPasswordInBytes, 1, salt, 0, SaltSize);

            byte[] a = new byte[32];
            Buffer.BlockCopy(hashedPasswordInBytes, 17, a, 0, 32);

            byte[] b;
            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, PBKDF2IterationCount))
            {
                b = rfc2898DeriveBytes.GetBytes(PBKDF2SubkeyLength);
            }

            return this.ByteArraysEqual(a, b);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool areArraysEqual = true;
            for (int index = 0; index < a.Length; ++index)
            {
                areArraysEqual &= a[index] == b[index];
            }

            return areArraysEqual;
        }
    }
}
