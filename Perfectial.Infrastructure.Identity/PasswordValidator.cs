namespace Perfectial.Infrastructure.Identity
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    public class PasswordValidator : IIdentityValidator<string>
    {
        public int RequiredLength { get; set; }
        public bool RequireNonLetterOrDigit { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireDigit { get; set; }

        public virtual Task<IdentityResult> ValidateAsync(string password)
        {
            var errors = new List<string>();
            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < this.RequiredLength)
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resource.PasswordTooShort, this.RequiredLength));
                }

                if (this.RequireNonLetterOrDigit && password.All(this.IsLetterOrDigit))
                {
                    errors.Add(Resource.PasswordRequireNonLetterOrDigit);
                }

                if (this.RequireDigit && password.All(c => !this.IsDigit(c)))
                {
                    errors.Add(Resource.PasswordRequireDigit);
                }

                if (this.RequireLowercase && password.All(c => !this.IsLower(c)))
                {
                    errors.Add(Resource.PasswordRequireLower);
                }

                if (this.RequireUppercase && password.All(c => !this.IsUpper(c)))
                {
                    errors.Add(Resource.PasswordRequireUpper);
                }
            }
            else
            {
                errors.Add(Resource.PasswordIsEmpty);
            }

            var validationResult = errors.Count == 0 ?
                new IdentityResult(true, null) :
                new IdentityResult(false, errors);

            return Task.FromResult(validationResult);
        }

        private bool IsDigit(char character)
        {
            if (character >= 48)
            {
                return character <= 57;
            }

            return false;
        }

        private bool IsLower(char character)
        {
            if (character >= 97)
            {
                return character <= 122;
            }

            return false;
        }

        private bool IsUpper(char character)
        {
            if (character >= 65)
            {
                return character <= 90;
            }

            return false;
        }

        private bool IsLetterOrDigit(char character)
        {
            if (!this.IsUpper(character) && !this.IsLower(character))
            {
                return this.IsDigit(character);
            }

            return true;
        }
    }
}
