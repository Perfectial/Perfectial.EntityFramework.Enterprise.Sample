namespace Perfectial.Infrastructure.Identity.Base
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyHashedPassword(string passwordHash, string password);
    }
}