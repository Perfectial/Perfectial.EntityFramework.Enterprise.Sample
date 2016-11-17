namespace Perfectial.Infrastructure.Identity.Model
{
    /// <summary>
    /// Default authentication types.
    /// </summary>
    public static class AuthenticationType
    {
        /// <summary>
        /// Default value for the main application cookie used by UseSignInCookies.
        /// </summary>
        public const string ApplicationCookie = "ApplicationCookie";

        /// <summary>
        /// Default value used for the ExternalSignInAuthenticationType configured by UseSignInCookies.
        /// </summary>
        public const string ExternalCookie = "ExternalCookie";

        /// <summary>
        /// Default value used by the UseOAuthBearerTokens method.
        /// </summary>
        public const string ExternalBearer = "ExternalBearer";

        /// <summary>
        /// Default value for authentication type used for two factor partial sign in.
        /// </summary>
        public const string TwoFactorCookie = "TwoFactorCookie";

        /// <summary>
        /// Default value for authentication type used for two factor remember browser.
        /// </summary>
        public const string TwoFactorRememberBrowserCookie = "TwoFactorRememberBrowser";
    }
}