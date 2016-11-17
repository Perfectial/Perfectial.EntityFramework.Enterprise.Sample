namespace Perfectial.Domain.Model
{
    using System;

    /// <summary>
    /// A shortcut of <see cref="EntityBase{TPrimaryKey}"/> for most used primary key type (<see cref="T:System.Int32"/>).
    /// </summary>
    [Serializable]
    public abstract class Entity : EntityBase<int>
    {
    }
}