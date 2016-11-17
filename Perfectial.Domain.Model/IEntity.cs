namespace Perfectial.Domain.Model
{
    /// <summary>
    /// Defines interface for base entity type. All entities in the system must implement this interface.
    /// </summary>
    /// <typeparam name="TPrimaryKey"> Type of the primary key of the entity. </typeparam>
    public interface IEntity<TPrimaryKey>
    {
        /// <summary>
        /// Gets or sets the unique identifier for this entity.
        /// </summary>
        /// <value> The unique identifier for this entity. </value>
        TPrimaryKey Id { get; set; }

        /// <summary>
        /// Checks if this entity is transient (not persisted to database and it has not an <see cref="Id"/>).
        /// </summary>
        /// <returns> True, if this entity is transient. </returns>
        bool IsTransient();
    }
}