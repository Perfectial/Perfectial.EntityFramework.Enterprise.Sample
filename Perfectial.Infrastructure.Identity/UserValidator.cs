namespace Perfectial.Infrastructure.Identity
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using EmailValidation;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Identity.Base;
    using Perfectial.Infrastructure.Identity.Model;

    public class UserValidator : IIdentityValidator<User>
    {
        private const string AlphaNumericUserNameRegexPattern = "^[A-Za-z0-9@_\\.]+$";
        private const string UserNamePropertyName = "UserName";
        private const string EmailPropertyName = "Email";

        private readonly IUserRepository userRepository;

        public bool AllowOnlyAlphanumericUserNames { get; set; }
        public bool RequireUniqueEmail { get; set; }

        public UserValidator(IUserRepository userRepository)
        {
            this.userRepository = userRepository;

            this.AllowOnlyAlphanumericUserNames = true;
        }

        public virtual async Task<IdentityResult> ValidateAsync(User user)
        {
            List<string> errors = new List<string>();

            errors.AddRange(await this.ValidateUserNameAsync(user));
            if (this.RequireUniqueEmail)
            {
                errors.AddRange(await this.ValidateEmailAsync(user));
            }

            var validationResult = errors.Count == 0 ?
                new IdentityResult(true, null) :
                new IdentityResult(false, errors);

            return validationResult;
        }

        private async Task<IEnumerable<string>> ValidateUserNameAsync(User user)
        {
            var errors = new List<string>();
            if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                if (this.AllowOnlyAlphanumericUserNames && !Regex.IsMatch(user.UserName, AlphaNumericUserNameRegexPattern))
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resource.InvalidUserName, user.UserName));
                }
                else
                {
                    var existingUser = await this.userRepository.FindByNameAsync(user.UserName);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        errors.Add(string.Format(CultureInfo.CurrentCulture, Resource.DuplicateName, user.UserName));
                    }
                }
            }
            else
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, Resource.PropertyTooShort, UserNamePropertyName));
            }

            return errors;
        }

        private async Task<IEnumerable<string>> ValidateEmailAsync(User user)
        {
            var errors = new List<string>();
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                if (!EmailValidator.Validate(user.Email))
                {
                    errors.Add(string.Format(CultureInfo.CurrentCulture, Resource.InvalidEmail, user.Email));
                }
                else
                {
                    var existingUser = await this.userRepository.FindByEmailAsync(user.UserName);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        errors.Add(string.Format(CultureInfo.CurrentCulture, Resource.DuplicateEmail, user.Email));
                    }
                }
            }
            else
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, Resource.PropertyTooShort, EmailPropertyName));
            }

            return errors;
        }
    }
}
