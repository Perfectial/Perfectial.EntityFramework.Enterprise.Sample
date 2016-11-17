namespace Perfectial.Infrastructure.Persistence.EntityFramework
{
    using System.Data.Entity;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence.Base;

    public interface IApplicationDbContext : IDbContext
    {
        IDbSet<ToDoItem> ToDoItems { get; set; }
        IDbSet<User> Users { get; set; }
        IDbSet<Role> Roles { get; set; }
    }
}