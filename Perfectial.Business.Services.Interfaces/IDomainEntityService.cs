using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfectial.Infrastructure.Services.Interfaces
{
    public interface IEntityService<TEntity> where TEntity : class
    {
        Task<int> DeleteAsync(int id);
    }
}
