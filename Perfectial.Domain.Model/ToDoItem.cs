namespace Perfectial.Domain.Model
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ToDoItem : EntityBase<int>
    {
        public ToDoItem()
        {
            this.CreationTime = DateTime.UtcNow;
            this.State = ToDoItemState.Active;
        }

        [ForeignKey("AssignedUserId")]
        public virtual User AssignedUser { get; set; }

        public virtual string AssignedUserId { get; set; }

        public string Description { get; set; }

        public DateTime CreationTime { get; set; }
        public ToDoItemState State { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is ToDoItem))
            {
                return false;
            }

            if (object.ReferenceEquals((object)this, obj))
            {
                return true;
            }

            ToDoItem entity = (ToDoItem)obj;
            if (this.IsTransient() && entity.IsTransient())
            {
                return false;
            }

            Type type1 = this.GetType();
            Type type2 = entity.GetType();
            if (!type1.IsAssignableFrom(type2) && !type2.IsAssignableFrom(type1))
            {
                return false;
            }

            return this.Equals((ToDoItem)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)this.State;
                hashCode = (hashCode * 397) ^ this.CreationTime.GetHashCode();
                return hashCode;
            }
        }

        protected bool Equals(ToDoItem other)
        {
            return base.Equals(other)
                && string.Equals(this.Description, other.Description)
                && this.State == other.State
                && string.Equals(this.CreationTime.ToString("G"), other.CreationTime.ToString("G"));
        }
    }
}
