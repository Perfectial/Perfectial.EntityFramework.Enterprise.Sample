namespace Perfectial.Domain.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Basic implementation of IEntity interface. An entity can inherit this class of directly implement to IEntity interface.
    /// </summary>
    /// <typeparam name="TPrimaryKey"> Type of the primary key of the entity. </typeparam>
    [Serializable]
    public abstract class EntityBase<TPrimaryKey> : IEntity<TPrimaryKey>
    {
        public virtual TPrimaryKey Id { get; set; }

        public static bool operator ==(EntityBase<TPrimaryKey> left, EntityBase<TPrimaryKey> right)
        {
            if (object.Equals(left, null))
            {
                return object.Equals(right, null);
            }

            return left.Equals((object)right);
        }

        public static bool operator !=(EntityBase<TPrimaryKey> left, EntityBase<TPrimaryKey> right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EntityBase<TPrimaryKey>))
            {
                return false;
            }

            if (object.ReferenceEquals((object)this, obj))
            {
                return true;
            }

            EntityBase<TPrimaryKey> entityBase = (EntityBase<TPrimaryKey>)obj;
            if (this.IsTransient() && entityBase.IsTransient())
            {
                return false;
            }

            Type type1 = this.GetType();
            Type type2 = entityBase.GetType();
            if (!type1.IsAssignableFrom(type2) && !type2.IsAssignableFrom(type1))
            {
                return false;
            }

            return this.Id.Equals((object)entityBase.Id);
        }

        public virtual bool IsTransient()
        {
            return EqualityComparer<TPrimaryKey>.Default.Equals(this.Id, default(TPrimaryKey));
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TPrimaryKey>.Default.GetHashCode(this.Id);
        }

        protected bool Equals(EntityBase<TPrimaryKey> other)
        {
            return EqualityComparer<TPrimaryKey>.Default.Equals(this.Id, other.Id);
        }
    }
}