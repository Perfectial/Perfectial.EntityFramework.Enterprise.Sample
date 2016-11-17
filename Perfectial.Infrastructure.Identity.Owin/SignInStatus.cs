namespace Perfectial.Infrastructure.Identity.Owin
{
    public enum SignInStatus
    {
        Success,
        LockedOut,
        RequiresVerification,
        Failure,
    }
}