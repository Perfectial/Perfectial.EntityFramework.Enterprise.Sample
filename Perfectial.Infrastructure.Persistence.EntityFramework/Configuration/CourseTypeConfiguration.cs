namespace Perfectial.Infrastructure.Persistence.EntityFramework.Configuration
{
    using System.Data.Entity.ModelConfiguration;

    using Perfectial.Domain.Model;

    public class CourseTypeConfiguration : EntityTypeConfiguration<Course>
    {
        public CourseTypeConfiguration()
        {
            this.HasKey(t => t.Id);

            this.HasRequired(entity => entity.Teacher).WithMany(entity => entity.Courses).HasForeignKey(entity => entity.TeacherId);

            this.ToTable("Course");
        }
    }
}
