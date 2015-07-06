using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;

namespace Perfectial.DataAccess.EntityFramework.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TEntity> ApplyIncludes<TEntity>(this IQueryable<TEntity> query, params Expression<Func<TEntity, object>>[] includes)
           where TEntity : class
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> query, int pageIndex, int pageSize)
            where TEntity : class
        {
            query = query.Skip(pageIndex).Take(pageSize);

            return query;
        }
    }
}
