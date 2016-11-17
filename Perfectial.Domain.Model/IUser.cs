namespace Perfectial.Domain.Model
{
    /// <summary>
    /// Defines interface for base user entity.
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Gets or sets the name for this user.
        /// </summary>
        /// <value> The name for this user. </value>
        string UserName { get; set; }
    }
}
