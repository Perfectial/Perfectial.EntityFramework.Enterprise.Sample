namespace Perfectial.Application.Model
{
    /// <summary>
    /// Defines common properties for entity based DTOs.
    /// </summary>
    /// <typeparam name="TPrimaryKey"> Type of the primary key of the entity. </typeparam>
    public interface IEntityDto<TPrimaryKey> : IDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for this entity.
        /// </summary>
        /// <value> The unique identifier for this entity. </value>
        TPrimaryKey Id { get; set; }
    }
}
