using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using Perfectial.DataAccess.DatabaseContext;
using Perfectial.Core.Domain.Base;

namespace LaunchPoint.DataAccess
{
    public static class ModelConfig
    {
        public static void BuildModel(DbModelBuilder builder)
        {
            ConfigureConventions(builder);

            ConfigureEntities(builder);

            ConfigureRelations(builder);

            ConfigureConstraints(builder);
        }

        private static void ConfigureConventions(DbModelBuilder builder)
        {
            builder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            builder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            // Overrides for the convention-based mappings.
            // We're assuming that all our fluent mappings are declared in this assembly.
            builder.Configurations.AddFromAssembly(Assembly.GetAssembly(typeof(DataContext)));
        }

        /// <summary>
        /// Automatic context's registration for all of domain entities, 
        /// disable it for scenario when store model are different from your domain,
        /// and add models to the context manually.
        /// You can add different models then the domain ones to DB if needed 
        /// </summary>
        /// <param name="builder">Model builder</param>
        private static void ConfigureEntities(DbModelBuilder builder)
        {
            var baseEntityType = typeof(BaseEntity);
            var domainAssembly = Assembly.GetAssembly(baseEntityType);

            var entitiesTypes = domainAssembly.ExportedTypes
                .Where(t => t.IsClass && t != baseEntityType && baseEntityType.IsAssignableFrom(t));

            foreach (var type in entitiesTypes)
            {
                var entityMethod = builder.GetType().GetMethod("Entity").MakeGenericMethod(new Type[] { type });
                entityMethod.Invoke(builder, null);
            }
        }

        private static void ConfigureRelations(DbModelBuilder builder)
        { }

        private static void ConfigureConstraints(DbModelBuilder builder)
        { }
    }
}
