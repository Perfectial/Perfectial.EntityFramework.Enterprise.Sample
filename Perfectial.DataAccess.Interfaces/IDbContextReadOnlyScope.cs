using Perfectial.Core.Domain.Models.Base;
using Perfectial.Core.Repository;
/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;

namespace Perfectial.DataAccess.Interfaces
{
    /// <summary>
    /// A read-only DbContextScope. Refer to the comments for IDbContextScope
    /// for more details.
    /// </summary>
    public interface IDbContextReadOnlyScope : IDisposable
    {
        /// <summary>
        /// Instantiate concreate repository
        /// </summary>
        /// <typeparam name="TRepository">Repository type</typeparam>
        /// <returns></returns>
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : AggregateRoot<TEntity>;
    }
}