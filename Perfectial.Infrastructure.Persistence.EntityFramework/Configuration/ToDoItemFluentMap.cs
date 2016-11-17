namespace Perfectial.Infrastructure.Persistence.EntityFramework.Configuration
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration;

    using Perfectial.Domain.Model;

    public class ToDoItemFluentMap : EntityTypeConfiguration<ToDoItem>
    {
        public ToDoItemFluentMap()
        {
            this.HasKey(t => t.Id);

            this.Property(entity => entity.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Property(entity => entity.Description).IsRequired();

            this.ToTable("ToDoItem");

            // TODO: Extend with Constraints, References configuration.
        }
    }
}
