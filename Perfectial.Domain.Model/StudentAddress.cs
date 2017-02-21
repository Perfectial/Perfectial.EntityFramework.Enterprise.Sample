namespace Perfectial.Domain.Model
{
    using System;

    public class StudentAddress : EntityBase<int>
    {
        public StudentAddress()
        {
        }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public virtual Student Student { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is StudentAddress))
            {
                return false;
            }

            if (object.ReferenceEquals((object)this, obj))
            {
                return true;
            }

            StudentAddress entity = (StudentAddress)obj;
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

            return this.Equals((StudentAddress)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Address1?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.Address2?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.City?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.State?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        protected bool Equals(StudentAddress other)
        {
            return base.Equals(other) 
                && string.Equals(this.Address1, other.Address1) 
                && string.Equals(this.Address2, other.Address2) 
                && string.Equals(this.City, other.City) 
                && string.Equals(this.State, other.State);
        }
    }
}
