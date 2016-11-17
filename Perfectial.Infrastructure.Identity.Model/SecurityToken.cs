namespace Perfectial.Infrastructure.Identity.Model
{
    public sealed class SecurityToken
    {
        private readonly byte[] data;

        public SecurityToken(byte[] data)
        {
            this.data = (byte[])data.Clone();
        }

        public byte[] GetDataNoClone()
        {
            return this.data;
        }
    }
}