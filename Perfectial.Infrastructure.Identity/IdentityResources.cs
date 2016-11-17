namespace Perfectial.Infrastructure.Identity
{
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A strongly-typed resource class, for looking up localized strings, etc.
    /// 
    /// </summary>
    [DebuggerNonUserCode]
    [CompilerGenerated]
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    internal class IdentityResources
    {
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        /// <summary>
        /// Returns the cached ResourceManager instance used by this class.
        /// 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals((object)IdentityResources.resourceMan, (object)null))
                    IdentityResources.resourceMan = new ResourceManager("Microsoft.AspNet.Identity.EntityFramework.IdentityResources", typeof(IdentityResources).Assembly);
                return IdentityResources.resourceMan;
            }
        }

        /// <summary>
        /// Overrides the current thread's CurrentUICulture property for all
        ///               resource lookups using this strongly typed resource class.
        /// 
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return IdentityResources.resourceCulture;
            }
            set
            {
                IdentityResources.resourceCulture = value;
            }
        }

        /// <summary>
        /// Looks up a localized string similar to Database Validation failed..
        /// 
        /// </summary>
        internal static string DbValidationFailed
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("DbValidationFailed", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to Email {0} is already taken..
        /// 
        /// </summary>
        internal static string DuplicateEmail
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("DuplicateEmail", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to User name {0} is already taken..
        /// 
        /// </summary>
        internal static string DuplicateUserName
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("DuplicateUserName", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to Entity Type {0} failed validation..
        /// 
        /// </summary>
        internal static string EntityFailedValidation
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("EntityFailedValidation", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to A user with that external login already exists..
        /// 
        /// </summary>
        internal static string ExternalLoginExists
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("ExternalLoginExists", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to The model backing the 'ApplicationDbContext' context has changed since the database was created. This could have happened because the model used by ASP.NET Identity Framework has changed or the model being used in your application has changed. To resolve this issue, you need to update your database. Consider using Code First Migrations to update the database (http://go.microsoft.com/fwlink/?LinkId=301867).  Before you update your database using Code First Migrations, please disable the schema consistency ch [rest of string was truncated]";.
        /// 
        /// </summary>
        internal static string IdentityV1SchemaError
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("IdentityV1SchemaError", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to Incorrect type, expected type of {0}..
        /// 
        /// </summary>
        internal static string IncorrectType
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("IncorrectType", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to {0} cannot be null or empty..
        /// 
        /// </summary>
        internal static string PropertyCannotBeEmpty
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("PropertyCannotBeEmpty", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to Role {0} already exists..
        /// 
        /// </summary>
        internal static string RoleAlreadyExists
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("RoleAlreadyExists", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to Role is not empty..
        /// 
        /// </summary>
        internal static string RoleIsNotEmpty
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("RoleIsNotEmpty", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to Role {0} does not exist..
        /// 
        /// </summary>
        internal static string RoleNotFound
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("RoleNotFound", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to User already in role..
        /// 
        /// </summary>
        internal static string UserAlreadyInRole
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("UserAlreadyInRole", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to The UserId cannot be found..
        /// 
        /// </summary>
        internal static string UserIdNotFound
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("UserIdNotFound", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to UserLogin already exists for loginProvider: {0} with providerKey: {1}.
        /// 
        /// </summary>
        internal static string UserLoginAlreadyExists
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("UserLoginAlreadyExists", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to User {0} does not exist..
        /// 
        /// </summary>
        internal static string UserNameNotFound
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("UserNameNotFound", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to User is not in role..
        /// 
        /// </summary>
        internal static string UserNotInRole
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("UserNotInRole", IdentityResources.resourceCulture);
            }
        }

        /// <summary>
        /// Looks up a localized string similar to Value cannot be null or empty..
        /// 
        /// </summary>
        internal static string ValueCannotBeNullOrEmpty
        {
            get
            {
                return IdentityResources.ResourceManager.GetString("ValueCannotBeNullOrEmpty", IdentityResources.resourceCulture);
            }
        }

        internal IdentityResources()
        {
        }
    }
}