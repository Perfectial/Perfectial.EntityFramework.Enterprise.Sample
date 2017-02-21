namespace Perfectial.Infrastructure.Persistence.EntityFramework.Configuration
{
    using System.Data.Entity.ModelConfiguration;

    using Perfectial.Domain.Model;

    public class StandardTypeConfiguration : EntityTypeConfiguration<Standard>
    {
        public StandardTypeConfiguration()
        {
            this.HasKey(t => t.Id);

            this.ToTable("Standard");
        }
    }
}
