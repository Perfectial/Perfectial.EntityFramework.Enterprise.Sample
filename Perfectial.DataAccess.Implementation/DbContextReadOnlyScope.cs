﻿using Perfectial.Core.Domain.Models.Base;
using Perfectial.Core.Repository;
/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using Perfectial.DataAccess.Interfaces;
using System.Data;

namespace Perfectial.DataAccess.Implementation
{
    public class DbContextReadOnlyScope : IDbContextReadOnlyScope
    {
        private DbContextScope _internalScope;

        public IDbContextCollection DbContexts { get { return _internalScope.DbContexts; } }

        public DbContextReadOnlyScope(IDbContextFactory dbContextFactory = null)
            : this(joiningOption: DbContextScopeOption.JoinExisting, isolationLevel: null, dbContextFactory: dbContextFactory)
        {}

        public DbContextReadOnlyScope(IsolationLevel isolationLevel, IDbContextFactory dbContextFactory = null)
            : this(joiningOption: DbContextScopeOption.ForceCreateNew, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory)
        { }

        public DbContextReadOnlyScope(DbContextScopeOption joiningOption, IsolationLevel? isolationLevel, IDbContextFactory dbContextFactory = null)
        {
            _internalScope = new DbContextScope(joiningOption: joiningOption, readOnly: true, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory);
        }

        public void Dispose()
        {
            _internalScope.Dispose();
        }

        IRepository<TEntity> IDbContextReadOnlyScope.GetRepository<TEntity>()
        {
            throw new System.NotImplementedException();
        }
    }
}