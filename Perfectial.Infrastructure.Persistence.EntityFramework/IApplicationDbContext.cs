namespace Perfectial.Infrastructure.Persistence.EntityFramework
{
    using System.Data.Entity;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence.Base;

    public interface IApplicationDbContext : IDbContext
    {
        IDbSet<Student> Students { get; set; }
        IDbSet<StudentAddress> StudentAddresses { get; set; }
        IDbSet<Standard> Standards { get; set; }
        IDbSet<Teacher> Teachers { get; set; }
        IDbSet<Course> Courses { get; set; }

        IDbSet<ToDoItem> ToDoItems { get; set; }
        IDbSet<User> Users { get; set; }
        IDbSet<Role> Roles { get; set; }
    }
}