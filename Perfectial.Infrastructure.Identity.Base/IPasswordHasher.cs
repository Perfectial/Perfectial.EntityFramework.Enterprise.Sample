namespace Perfectial.Infrastructure.Identity.Base
{
    using Perfectial.Infrastructure.Identity.Model;

    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyHashedPassword(string passwordHash, string password);
    }
}