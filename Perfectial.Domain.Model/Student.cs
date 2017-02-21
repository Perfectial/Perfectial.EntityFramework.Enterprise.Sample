namespace Perfectial.Domain.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Student : EntityBase<int>
    {
        public Student()
        {
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public int? StandardId { get; set; }

        public virtual Standard Standard { get; set; }
        public virtual StudentAddress StudentAddress { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

        public override bool Equals(object obj)
        {
            if (!(obj is Student))
            {
                return false;
            }

            if (object.ReferenceEquals((object)this, obj))
            {
                return true;
            }

            Student entity = (Student)obj;
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

            return this.Equals((Student)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.Description?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ this.StandardId.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Standard?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.StudentAddress?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.Courses?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        protected bool Equals(Student other)
        {
            return base.Equals(other) 
                && string.Equals(this.Name, other.Name) 
                && string.Equals(this.Description, other.Description) 
                && this.StandardId == other.StandardId 
                && object.Equals(this.Standard, other.Standard) 
                && object.Equals(this.StudentAddress, other.StudentAddress) 
                && this.Courses.SequenceEqual(other.Courses);
        }
    }
}
