namespace Perfectial.Infrastructure.Persistence.EntityFramework.Configuration
{
    using System.Data.Entity.ModelConfiguration;

    using Perfectial.Domain.Model;

    public class StudentTypeConfiguration : EntityTypeConfiguration<Student>
    {
        public StudentTypeConfiguration()
        {
            this.HasKey(t => t.Id);

            this.HasRequired(entity => entity.StudentAddress).WithRequiredPrincipal(entity => entity.Student).WillCascadeOnDelete(true);
            this.HasOptional(entity => entity.Standard).WithMany(entity => entity.Students).HasForeignKey(entity => entity.StandardId);
            this.HasMany(entity => entity.Courses).WithMany(entity => entity.Students)
                .Map(entity =>
                    {
                        entity.MapLeftKey("StudentId");
                        entity.MapRightKey("CourseId");
                        entity.ToTable("StudentCourse");
                    });

            this.ToTable("Student");
        }
    }
}
