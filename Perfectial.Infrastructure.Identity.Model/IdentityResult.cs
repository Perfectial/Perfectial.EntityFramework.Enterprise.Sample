namespace Perfectial.Infrastructure.Identity.Model
{
    using System.Collections.Generic;

    public class IdentityResult
    {
        public bool IsValid { get; private set; }
        public IEnumerable<string> Errors { get; private set; }

        public IdentityResult(bool isValid, IEnumerable<string> errors)
        {
            this.IsValid = isValid;
            this.Errors = errors;
        }
    }
}
