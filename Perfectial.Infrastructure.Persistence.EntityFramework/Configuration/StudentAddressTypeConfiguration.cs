namespace Perfectial.Infrastructure.Persistence.EntityFramework.Configuration
{
    using System.Data.Entity.ModelConfiguration;

    using Perfectial.Domain.Model;

    public class StudentAddressTypeConfiguration : EntityTypeConfiguration<StudentAddress>
    {
        public StudentAddressTypeConfiguration()
        {
            this.HasKey(t => t.Id);

            this.HasRequired(entity => entity.Student).WithRequiredDependent(entity => entity.StudentAddress).WillCascadeOnDelete(true);

            this.ToTable("StudentAddress");
        }
    }
}
