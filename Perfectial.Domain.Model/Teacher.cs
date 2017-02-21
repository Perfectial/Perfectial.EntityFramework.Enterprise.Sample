namespace Perfectial.Domain.Model
{
    using System;
    using System.Collections.Generic;

    public class Teacher : EntityBase<int>
    {
        public Teacher()
        {
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int? StandardId { get; set; }
        public int? Type { get; set; }

        public virtual Standard Standard { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

        public override bool Equals(object obj)
        {
            if (!(obj is Teacher))
            {
                return false;
            }

            if (object.ReferenceEquals((object)this, obj))
            {
                return true;
            }

            Teacher entity = (Teacher)obj;
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

            return this.Equals((Teacher)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ this.StandardId.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Type.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Standard?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        protected bool Equals(Teacher other)
        {
            return base.Equals(other) 
                && string.Equals(this.Name, other.Name) 
                && string.Equals(this.Description, other.Description) 
                && this.StandardId == other.StandardId 
                && this.Type == other.Type 
                && object.Equals(this.Standard, other.Standard);
        }
    }
}
