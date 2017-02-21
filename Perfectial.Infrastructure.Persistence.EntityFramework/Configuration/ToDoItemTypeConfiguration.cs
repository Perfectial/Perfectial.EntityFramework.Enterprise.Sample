namespace Perfectial.Infrastructure.Persistence.EntityFramework.Configuration
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration;

    using Perfectial.Domain.Model;

    public class ToDoItemTypeConfiguration : EntityTypeConfiguration<ToDoItem>
    {
        public ToDoItemTypeConfiguration()
        {
            this.HasKey(t => t.Id);

            this.Property(entity => entity.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(entity => entity.Description).IsRequired();

            this.HasOptional(entity => entity.AssignedUser).WithMany(entity => entity.ToDoItems).HasForeignKey(entity => entity.AssignedUserId);

            this.ToTable("ToDoItem");
        }
    }
}
