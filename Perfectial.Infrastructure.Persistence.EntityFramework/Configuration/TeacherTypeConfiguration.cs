namespace Perfectial.Infrastructure.Persistence.EntityFramework.Configuration
{
    using System.Data.Entity.ModelConfiguration;

    using Perfectial.Domain.Model;

    public class TeacherTypeConfiguration : EntityTypeConfiguration<Teacher>
    {
        public TeacherTypeConfiguration()
        {
            this.HasKey(t => t.Id);

            this.HasRequired(entity => entity.Standard).WithMany(entity => entity.Teachers).HasForeignKey(entity => entity.StandardId);

            this.ToTable("Teacher");
        }
    }
}
