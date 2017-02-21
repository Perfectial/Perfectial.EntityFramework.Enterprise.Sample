namespace Perfectial.Domain.Model
{
    using System;
    using System.Collections.Generic;

    public class Course : EntityBase<int>
    {
        public Course()
        {
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int? TeacherId { get; set; }

        public virtual Teacher Teacher { get; set; }

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        public override bool Equals(object obj)
        {
            if (!(obj is Course))
            {
                return false;
            }

            if (object.ReferenceEquals((object)this, obj))
            {
                return true;
            }

            Course entity = (Course)obj;
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

            return this.Equals((Course)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ this.TeacherId.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Teacher?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        protected bool Equals(Course other)
        {
            return base.Equals(other) 
                && string.Equals(this.Name, other.Name) 
                && string.Equals(this.Description, other.Description) 
                && this.TeacherId == other.TeacherId 
                && object.Equals(this.Teacher, other.Teacher);
        }
    }
}
