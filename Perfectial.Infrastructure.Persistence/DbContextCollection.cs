/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

namespace Perfectial.Infrastructure.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Perfectial.Infrastructure.Persistence.Base;

    using IDbTransaction = Perfectial.Infrastructure.Persistence.Base.IDbTransaction;

    /// <summary>
    /// As its name suggests, DbContextCollection maintains a collection of DbContext instances.
    /// 
    /// What it does in a nutshell:
    /// - Lazily instantiates DbContext instances when its Get Of TDbContext () method is called
    /// (and optionally starts an explicit database transaction).
    /// - Keeps track of the DbContext instances it created so that it can return the existing
    /// instance when asked for a DbContext of a specific type.
    /// - Takes care of committing / rolling back changes and transactions on all the DbContext
    /// instances it created when its Commit() or Rollback() method is called.
    /// 
    /// </summary>
    public class DbContextCollection : IDbContextCollection
    {
        private readonly Dictionary<Type, IDbContext> initializedDbContexts;
        private readonly Dictionary<IDbContext, IDbTransaction> transactions;
        private readonly IDbContextFactory dbContextFactory;
        private readonly bool readOnly;

        private IsolationLevel? isolationLevel;
        private bool disposed;
        private bool completed;

        public DbContextCollection(bool readOnly = false, IsolationLevel? isolationLevel = null, IDbContextFactory dbContextFactory = null)
        {
            this.disposed = false;
            this.completed = false;

            this.initializedDbContexts = new Dictionary<Type, IDbContext>();
            this.transactions = new Dictionary<IDbContext, IDbTransaction>();

            this.readOnly = readOnly;
            this.isolationLevel = isolationLevel;
            this.dbContextFactory = dbContextFactory;
        }

        internal Dictionary<Type, IDbContext> InitializedDbContexts => this.initializedDbContexts;

        public TDbContext Get<TDbContext>() where TDbContext : class, IDbContext
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("DbContextCollection");
            }

            var requestedType = typeof(TDbContext);
            if (!this.initializedDbContexts.ContainsKey(requestedType))
            {
                // First time we've been asked for this particular DbContext type.
                // Create one, cache it and start its database transaction if needed.
                var dbContext = this.dbContextFactory != null
                    ? this.dbContextFactory.CreateDbContext<TDbContext>()
                    : Activator.CreateInstance<TDbContext>();

                this.initializedDbContexts.Add(requestedType, dbContext);

                if (this.readOnly)
                {
                    dbContext.AutoDetectChanges(false);
                }

                if (this.isolationLevel.HasValue)
                {
                    var transaction = dbContext.BeginTransaction(this.isolationLevel.Value);
                    this.transactions.Add(dbContext, transaction);
                }
            }

            return this.initializedDbContexts[requestedType] as TDbContext;
        }

        public int Commit()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("DbContextCollection");
            }

            if (this.completed)
            {
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DbContextCollection. All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DbContextCollection and make your changes there.");
            }

            // Best effort. You'll note that we're not actually implementing an atomic commit 
            // here. It entirely possible that one DbContext instance will be committed successfully
            // and another will fail. Implementing an atomic commit would require us to wrap
            // all of this in a TransactionScope. The problem with TransactionScope is that 
            // the database transaction it creates may be automatically promoted to a 
            // distributed transaction if our DbContext instances happen to be using different 
            // databases. And that would require the DTC service (Distributed Transaction Coordinator)
            // to be enabled on all of our live and dev servers as well as on all of our dev workstations.
            // Otherwise the whole thing would blow up at runtime. 

            // In practice, if our services are implemented following a reasonably DDD approach,
            // a business transaction (i.e. a service method) should only modify entities in a single
            // DbContext. So we should never find ourselves in a situation where two DbContext instances
            // contain uncommitted changes here. We should therefore never be in a situation where the below
            // would result in a partial commit. 
            ExceptionDispatchInfo lastError = null;

            var commitResult = 0;
            foreach (var dbContext in this.initializedDbContexts.Values)
            {
                try
                {
                    if (!this.readOnly)
                    {
                        commitResult += dbContext.SaveChanges();
                    }

                    // If we've started an explicit database transaction, time to commit it now.
                    var transaction = GetValueOrDefault(this.transactions, dbContext);
                    if (transaction != null)
                    {
                        transaction.Commit();
                        transaction.Dispose();
                    }
                }
                catch (Exception exception)
                {
                    lastError = ExceptionDispatchInfo.Capture(exception);
                }
            }

            this.transactions.Clear();
            this.completed = true;

            lastError?.Throw(); // Re-throw while maintaining the exception's original stack track

            return commitResult;
        }

        public Task<int> CommitAsync()
        {
            return this.CommitAsync(CancellationToken.None);
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("DbContextCollection");
            }

            if (this.completed)
            {
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DbContextCollection. All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DbContextCollection and make your changes there.");
            }

            // See comments in the sync version of this method for more details.
            ExceptionDispatchInfo lastError = null;

            var commitResult = 0;
            foreach (var dbContext in this.initializedDbContexts.Values)
            {
                try
                {
                    if (!this.readOnly)
                    {
                        commitResult += await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }

                    // If we've started an explicit database transaction, time to commit it now.
                    var transaction = GetValueOrDefault(this.transactions, dbContext);
                    if (transaction != null)
                    {
                        transaction.Commit();
                        transaction.Dispose();
                    }
                }
                catch (Exception e)
                {
                    lastError = ExceptionDispatchInfo.Capture(e);
                }
            }

            this.transactions.Clear();
            this.completed = true;

            lastError?.Throw(); // Re-throw while maintaining the exception's original stack track

            return commitResult;
        }

        public void Rollback()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("DbContextCollection");
            }

            if (this.completed)
            {
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DbContextCollection. All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DbContextCollection and make your changes there.");
            }

            ExceptionDispatchInfo lastError = null;

            foreach (var dbContext in this.initializedDbContexts.Values)
            {
                // There's no need to explicitly rollback changes in a DbContext as
                // DbContext doesn't save any changes until its SaveChanges() method is called.
                // So "rolling back" for a DbContext simply means not calling its SaveChanges()
                // method. 

                // But if we've started an explicit database transaction, then we must roll it back.
                var transaction = GetValueOrDefault(this.transactions, dbContext);
                if (transaction != null)
                {
                    try
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                    }
                    catch (Exception e)
                    {
                        lastError = ExceptionDispatchInfo.Capture(e);
                    }
                }
            }

            this.transactions.Clear();
            this.completed = true;

            lastError?.Throw(); // Re-throw while maintaining the exception's original stack track
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            // Do our best here to dispose as much as we can even if we get errors along the way.
            // Now is not the time to throw. Correctly implemented applications will have called
            // either Commit() or Rollback() first and would have got the error there.
            if (!this.completed)
            {
                try
                {
                    if (this.readOnly)
                    {
                        this.Commit();
                    }
                    else
                    {
                        this.Rollback();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            foreach (var dbContext in this.initializedDbContexts.Values)
            {
                try
                {
                    dbContext.Dispose();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            this.initializedDbContexts.Clear();
            this.disposed = true;
        }

        /// <summary>
        /// Returns the value associated with the specified key or the default 
        /// value for the TValue type.
        /// </summary>
        private static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }
    }
}