namespace Perfectial.Infrastructure.Persistence.EntityFramework
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Reflection;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence.EntityFramework.Configuration;

    public class ModelConfiguration
    {
        public static void BuildModel(DbModelBuilder modelBuilder)
        {
            ConfigureConventions(modelBuilder);
            ConfigureRelations(modelBuilder);
            ConfigureConstraints(modelBuilder);

            EntityTypeConfiguration<User> userConfiguration = modelBuilder.Entity<User>().ToTable("User");
            userConfiguration.HasMany(u => u.UserRoles).WithRequired().HasForeignKey(role => role.UserId);
            userConfiguration.HasMany(u => u.Claims).WithRequired().HasForeignKey(claim => claim.UserId);
            userConfiguration.HasMany(u => u.Logins).WithRequired().HasForeignKey(login => login.UserId);

            StringPropertyConfiguration propertyUserNameConfiguration = userConfiguration.Property(u => u.UserName).IsRequired().HasMaxLength(256);
            string annotationUserName = "Index";
            IndexAttribute annotationUserNameIndexAttribute = new IndexAttribute("UserNameIndex") { IsUnique = true };
            IndexAnnotation annotationUserNameIndex = new IndexAnnotation(annotationUserNameIndexAttribute);
            propertyUserNameConfiguration.HasColumnAnnotation(annotationUserName, annotationUserNameIndex);

            userConfiguration.Property(u => u.Email).HasMaxLength(256);

            modelBuilder.Entity<UserRole>().HasKey(role => new { role.UserId, role.RoleId });
            modelBuilder.Entity<UserRole>().ToTable("UsersRoles");

            modelBuilder.Entity<UserLogin>().HasKey(login => new { login.LoginProvider, login.ProviderKey, login.UserId });
            modelBuilder.Entity<UserLogin>().ToTable("UserLogin");

            modelBuilder.Entity<UserClaim>().ToTable("UserClaim");

            EntityTypeConfiguration<Role> roleConfiguration = modelBuilder.Entity<Role>().ToTable("Role");
            StringPropertyConfiguration propertyNameConfiguration = roleConfiguration.Property(r => r.Name).IsRequired().HasMaxLength(256);
            string annotationName = "Index";
            IndexAttribute annotationNameIndexAttribute = new IndexAttribute("RoleNameIndex") { IsUnique = true };
            IndexAnnotation annotationNameIndex = new IndexAnnotation(annotationNameIndexAttribute);
            propertyNameConfiguration.HasColumnAnnotation(annotationName, annotationNameIndex);

            roleConfiguration.HasMany(role => role.UserRoles).WithRequired().HasForeignKey(userRole => userRole.RoleId);
        }

        private static void ConfigureConventions(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            // Overrides for the convention-based mappings.
            // We're assuming that all our fluent mappings are declared in this assembly.
            // modelBuilder.Configurations.AddFromAssembly(Assembly.GetAssembly(typeof(ApplicationDbContext)));
            modelBuilder.Configurations.Add(new ToDoItemTypeConfiguration());
            modelBuilder.Configurations.Add(new StudentTypeConfiguration());
            modelBuilder.Configurations.Add(new StudentAddressTypeConfiguration());
            modelBuilder.Configurations.Add(new StandardTypeConfiguration());
            modelBuilder.Configurations.Add(new TeacherTypeConfiguration());
            modelBuilder.Configurations.Add(new CourseTypeConfiguration());
        }

        private static void ConfigureRelations(DbModelBuilder modelBuilder)
        {
        }

        private static void ConfigureConstraints(DbModelBuilder modelBuilder)
        {
        }
    }
}