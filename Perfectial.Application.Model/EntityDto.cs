namespace Perfectial.Application.Model
{
    using System;

    /// <summary>
    /// A shortcut of <see cref="EntityDtoBase{TPrimaryKey}"/> for most used primary key type (<see cref="T:System.Int32"/>).
    /// </summary>
    [Serializable]
    public class EntityDto : EntityDtoBase<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDto"/> class.
        /// </summary>
        public EntityDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDto"/> class.
        /// </summary>
        /// <param name="id"> The unique identifier for this entity. </param>
        public EntityDto(int id)
          : base(id)
        {
        }
    }
}
