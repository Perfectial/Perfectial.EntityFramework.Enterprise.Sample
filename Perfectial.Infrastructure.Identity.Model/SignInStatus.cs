namespace Perfectial.Infrastructure.Identity.Model
{
    public enum SignInStatus
    {
        Success,
        LockedOut,
        RequiresVerification,
        Failure,
    }
}