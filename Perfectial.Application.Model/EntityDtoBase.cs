namespace Perfectial.Application.Model
{
    using System;

    /// <summary>
    /// Implements common properties for entity based DTOs.
    /// </summary>
    /// <typeparam name="TPrimaryKey"> Type of the primary key. </typeparam>
    [Serializable]
    public class EntityDtoBase<TPrimaryKey> : IEntityDto<TPrimaryKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDtoBase{TPrimaryKey}"/> class.
        /// </summary>
        public EntityDtoBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDtoBase{TPrimaryKey}"/> class.
        /// </summary>
        /// <param name="id"> The unique identifier for this entity. </param>
        public EntityDtoBase(TPrimaryKey id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets or sets the unique identifier for this entity.
        /// </summary>
        /// <value> The unique identifier for this entity. </value>
        public TPrimaryKey Id { get; set; }
    }
}
