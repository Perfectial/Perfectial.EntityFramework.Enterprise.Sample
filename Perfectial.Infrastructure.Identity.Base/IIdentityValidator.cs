namespace Perfectial.Infrastructure.Identity.Base
{
    using System.Threading.Tasks;

    using Perfectial.Infrastructure.Identity.Model;

    public interface IIdentityValidator<in T>
    {
        Task<IdentityResult> ValidateAsync(T item);
    }
}