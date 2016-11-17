namespace Perfectial.Infrastructure.Identity
{
    using System.Globalization;
    using System.Threading.Tasks;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    public class MinimumLengthValidator : IIdentityValidator<string>
    {
        public int RequiredLength { get; set; }

        public MinimumLengthValidator(int requiredLength)
        {
            this.RequiredLength = requiredLength;
        }

        public virtual Task<IdentityResult> ValidateAsync(string password)
        {
            IdentityResult validationResult;
            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length >= this.RequiredLength)
                {
                    validationResult = new IdentityResult(true, null);
                }
                else
                {
                    var error = string.Format(CultureInfo.CurrentCulture, Resource.PasswordTooShort, this.RequiredLength);
                    validationResult = new IdentityResult(false, new[] { error });
                }
            }
            else
            {
                var error = string.Format(CultureInfo.CurrentCulture, Resource.PasswordIsEmpty);
                validationResult = new IdentityResult(false, new[] { error });
            }


            return Task.FromResult(validationResult);
        }
    }
}