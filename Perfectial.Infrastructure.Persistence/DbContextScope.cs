﻿/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */

namespace Perfectial.Infrastructure.Persistence
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;
    using System.Threading.Tasks;

    using Perfectial.Infrastructure.Persistence.Base;

    public class DbContextScope : IDbContextScope
    {
        private static readonly string AmbientDbContextScopeKey = "AmbientDbcontext_" + Guid.NewGuid();

        // Use a ConditionalWeakTable instead of a simple ConcurrentDictionary to store our DbContextScope instances 
        // in order to prevent leaking DbContextScope instances if someone doesn't dispose them properly.
        //
        // For example, if we used a ConcurrentDictionary and someone let go of a DbContextScope instance without 
        // disposing it, our ConcurrentDictionary would still have a reference to it, preventing
        // the GC from being able to collect it => leak. With a ConditionalWeakTable, we don't hold a reference
        // to the DbContextScope instances we store in there, allowing them to get GCed.
        // The doc for ConditionalWeakTable isn't the best. This SO anser does a good job at explaining what 
        // it does: http://stackoverflow.com/a/18613811
        private static readonly ConditionalWeakTable<InstanceIdentifier, DbContextScope> DbContextScopeInstances = new ConditionalWeakTable<InstanceIdentifier, DbContextScope>();

        private readonly InstanceIdentifier instanceIdentifier = new InstanceIdentifier();

        private readonly bool readOnly;
        private readonly bool nested;

        private readonly DbContextScope parentScope;
        private readonly DbContextCollection dbContexts;

        private bool disposed;
        private bool completed;

        public DbContextScope(IDbContextFactory dbContextFactory = null)
            : this(
                joiningOption: DbContextScopeOption.JoinExisting,
                readOnly: false,
                isolationLevel: null,
                dbContextFactory: dbContextFactory)
        {
        }

        public DbContextScope(bool readOnly, IDbContextFactory dbContextFactory = null)
            : this(
                joiningOption: DbContextScopeOption.JoinExisting,
                readOnly: readOnly,
                isolationLevel: null,
                dbContextFactory: dbContextFactory)
        {
        }

        public DbContextScope(DbContextScopeOption joiningOption, bool readOnly, IsolationLevel? isolationLevel, IDbContextFactory dbContextFactory = null)
        {
            if (isolationLevel.HasValue && joiningOption == DbContextScopeOption.JoinExisting)
            {
                throw new ArgumentException("Cannot join an ambient DbContextScope when an explicit database transaction is required. When requiring explicit database transactions to be used (i.e. when the 'isolationLevel' parameter is set), you must not also ask to join the ambient context (i.e. the 'joinAmbient' parameter must be set to false).");
            }

            this.disposed = false;
            this.completed = false;
            this.readOnly = readOnly;

            this.parentScope = GetAmbientScope();
            if (this.parentScope != null && joiningOption == DbContextScopeOption.JoinExisting)
            {
                if (this.parentScope.readOnly && !this.readOnly)
                {
                    throw new InvalidOperationException("Cannot nest a read/write DbContextScope within a read-only DbContextScope.");
                }

                this.nested = true;
                this.dbContexts = this.parentScope.dbContexts;
            }
            else
            {
                this.nested = false;
                this.dbContexts = new DbContextCollection(readOnly, isolationLevel, dbContextFactory);
            }

            SetAmbientScope(this);
        }

        public IDbContextCollection DbContexts => this.dbContexts;

        public int SaveChanges()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("DbContextScope");
            }   

            if (this.completed)
            {
                throw new InvalidOperationException("You cannot call SaveChanges() more than once on a DbContextScope. A DbContextScope is meant to encapsulate a business transaction: create the scope at the start of the business transaction and then call SaveChanges() at the end. Calling SaveChanges() mid-way through a business transaction doesn't make sense and most likely mean that you should refactor your service method into two separate service method that each create their own DbContextScope and each implement a single business transaction.");
            }

            // Only save changes if we're not a nested scope. Otherwise, let the top-level scope 
            // decide when the changes should be saved.
            var c = 0;
            if (!this.nested)
            {
                c = this.CommitInternal();
            }

            this.completed = true;

            return c;
        }

        public Task<int> SaveChangesAsync()
        {
            return this.SaveChangesAsync(CancellationToken.None);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException("DbContextScope");
            }

            if (this.completed)
            {
                throw new InvalidOperationException("You cannot call SaveChanges() more than once on a DbContextScope. A DbContextScope is meant to encapsulate a business transaction: create the scope at the start of the business transaction and then call SaveChanges() at the end. Calling SaveChanges() mid-way through a business transaction doesn't make sense and most likely mean that you should refactor your service method into two separate service method that each create their own DbContextScope and each implement a single business transaction.");
            }

            // Only save changes if we're not a nested scope. Otherwise, let the top-level scope 
            // decide when the changes should be saved.
            var c = 0;
            if (!this.nested)
            {
                c = await this.CommitInternalAsync(cancellationToken).ConfigureAwait(false);
            }

            this.completed = true;

            return c;
        }

        public void RefreshEntitiesInParentScope(IEnumerable entities)
        {
            if (entities == null)
            {
                return;
            }

            if (this.parentScope == null)
            {
                return;
            }

            // The parent scope uses the same DbContext instances as we do - no need to refresh anything
            if (this.nested) 
            {
                return;
            }

            // OK, so we must loop through all the DbContext instances in the parent scope
            // and see if their first-level cache (i.e. their ObjectStateManager) contains the provided entities. 
            // If they do, we'll need to force a refresh from the database. 

            // I'm sorry for this code but it's the only way to do this with the current version of Entity Framework 
            // as far as I can see.

            // What would be much nicer would be to have a way to merge all the modified / added / deleted
            // entities from one DbContext instance to another. NHibernate has support for this sort of stuff 
            // but EF still lags behind in this respect. But there is hope: https://entityframework.codeplex.com/workitem/864

            // NOTE: DbContext implements the ObjectContext property of the IObjectContextAdapter interface explicitely.
            // So we must cast the DbContext instances to IObjectContextAdapter in order to access their ObjectContext.
            // This cast is completely safe.
            foreach (IDbContext contextInCurrentScope in this.dbContexts.InitializedDbContexts.Values)
            {
                var correspondingParentContext =
                    this.parentScope.dbContexts.InitializedDbContexts.Values.SingleOrDefault(parentContext => parentContext.GetType() == contextInCurrentScope.GetType());

                if (correspondingParentContext == null)
                {
                    continue; // No DbContext of this type has been created in the parent scope yet. So no need to refresh anything for this DbContext type.
                }

                // Both our scope and the parent scope have an instance of the same DbContext type. 
                // We can now look in the parent DbContext instance for entities that need to
                // be refreshed.
                foreach (var entityToRefresh in entities)
                {
                    // First, we need to find what the EntityKey for this entity is. 
                    // We need this EntityKey in order to check if this entity has
                    // already been loaded in the parent DbContext's first-level cache (the ObjectStateManager).
                    IDbObjectStateEntry stateInCurrentScope;
                    if (contextInCurrentScope.TryGetObjectStateEntry(entityToRefresh, out stateInCurrentScope))
                    {
                        var entityKey = stateInCurrentScope.EntityKey;

                        // Now we can see if that entity exists in the parent DbContext instance and refresh it.
                        IDbObjectStateEntry stateInParentScope;
                        if (correspondingParentContext.TryGetObjectStateEntry(entityKey, out stateInParentScope))
                        {
                            // Only refresh the entity in the parent DbContext from the database if that entity hasn't already been
                            // modified in the parent. Otherwise, let the whatever concurency rules the application uses
                            // apply.
                            if (stateInParentScope.State == DbObjectState.Unchanged)
                            {
                                correspondingParentContext.Refresh(stateInParentScope.Entity);
                            }
                        }
                    }
                }
            }
        }

        public async Task RefreshEntitiesInParentScopeAsync(IEnumerable entities)
        {
            // See comments in the sync version of this method for an explanation of what we're doing here.
            if (entities == null)
            {
                return;
            }

            if (this.parentScope == null)
            {
                return;
            }

            if (this.nested)
            {
                return;
            }

            foreach (IDbContext contextInCurrentScope in this.dbContexts.InitializedDbContexts.Values)
            {
                var correspondingParentContext =
                    this.parentScope.dbContexts.InitializedDbContexts.Values.SingleOrDefault(parentContext => parentContext.GetType() == contextInCurrentScope.GetType());

                if (correspondingParentContext == null)
                {
                    continue;
                }

                // Both our scope and the parent scope have an instance of the same DbContext type. 
                // We can now look in the parent DbContext instance for entities that need to
                // be refreshed.
                foreach (var entityToRefresh in entities)
                {
                    // First, we need to find what the EntityKey for this entity is. 
                    // We need this EntityKey in order to check if this entity has
                    // already been loaded in the parent DbContext's first-level cache (the ObjectStateManager).
                    IDbObjectStateEntry stateInCurrentScope;
                    if (contextInCurrentScope.TryGetObjectStateEntry(entityToRefresh, out stateInCurrentScope))
                    {
                        var entityKey = stateInCurrentScope.EntityKey;

                        // Now we can see if that entity exists in the parent DbContext instance and refresh it.
                        IDbObjectStateEntry stateInParentScope;
                        if (correspondingParentContext.TryGetObjectStateEntry(entityKey, out stateInParentScope))
                        {
                            // Only refresh the entity in the parent DbContext from the database if that entity hasn't already been
                            // modified in the parent. Otherwise, let the whatever concurency rules the application uses
                            // apply.
                            if (stateInParentScope.State == DbObjectState.Unchanged)
                            {
                                await correspondingParentContext.RefreshAsync(stateInParentScope.Entity).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            // Commit / Rollback and dispose all of our DbContext instances
            if (!this.nested)
            {
                if (!this.completed)
                {
                    // Do our best to clean up as much as we can but don't throw here as it's too late anyway.
                    try
                    {
                        if (this.readOnly)
                        {
                            // Disposing a read-only scope before having called its SaveChanges() method
                            // is the normal and expected behavior. Read-only scopes get committed automatically.
                            this.CommitInternal();
                        }
                        else
                        {
                            // Disposing a read/write scope before having called its SaveChanges() method
                            // indicates that something went wrong and that all changes should be rolled-back.
                            this.RollbackInternal();
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }

                    this.completed = true;
                }

                this.dbContexts.Dispose();
            }

            // Pop ourself from the ambient scope stack.
            var currentAmbientScope = GetAmbientScope();
            if (currentAmbientScope != this)
            {
                // This is a serious programming error. Worth throwing here.
                throw new InvalidOperationException("DbContextScope instances must be disposed of in the order in which they were created!");
            }

            RemoveAmbientScope();

            if (this.parentScope != null)
            {
                if (this.parentScope.disposed)
                {
                    /*
                     * If our parent scope has been disposed before us, it can only mean one thing:
                     * someone started a parallel flow of execution and forgot to suppress the
                     * ambient context before doing so. And we've been created in that parallel flow.
                     * 
                     * Since the CallContext flows through all async points, the ambient scope in the 
                     * main flow of execution ended up becoming the ambient scope in this parallel flow
                     * of execution as well. So when we were created, we captured it as our "parent scope". 
                     * 
                     * The main flow of execution then completed while our flow was still ongoing. When 
                     * the main flow of execution completed, the ambient scope there (which we think is our 
                     * parent scope) got disposed of as it should.
                     * 
                     * So here we are: our parent scope isn't actually our parent scope. It was the ambient
                     * scope in the main flow of execution from which we branched off. We should never have seen 
                     * it. Whoever wrote the code that created this parallel task should have suppressed
                     * the ambient context before creating the task - that way we wouldn't have captured
                     * this bogus parent scope.
                     * 
                     * While this is definitely a programming error, it's not worth throwing here. We can only 
                     * be in one of two scenario:
                     * 
                     * - If the developer who created the parallel task was mindful to force the creation of 
                     * a new scope in the parallel task (with IDbContextScopeFactory.CreateNew() instead of 
                     * JoinOrCreate()) then no harm has been done. We haven't tried to access the same DbContext
                     * instance from multiple threads.
                     * 
                     * - If this was not the case, they probably already got an exception complaining about the same
                     * DbContext or ObjectContext being accessed from multiple threads simultaneously (or a related
                     * error like multiple active result sets on a DataReader, which is caused by attempting to execute
                     * several queries in parallel on the same DbContext instance). So the code has already blow up.
                     * 
                     * So just record a warning here. Hopefully someone will see it and will fix the code.
                     */

                    var message = @"PROGRAMMING ERROR - When attempting to dispose a DbContextScope, we found that our parent DbContextScope has already been disposed! This means that someone started a parallel flow of execution (e.g. created a TPL task, created a thread or enqueued a work item on the ThreadPool) within the context of a DbContextScope without suppressing the ambient context first. 

                    In order to fix this:
                    1) Look at the stack trace below - this is the stack trace of the parallel task in question.
                    2) Find out where this parallel task was created.
                    3) Change the code so that the ambient context is suppressed before the parallel task is created. You can do this with IDbContextScopeFactory.SuppressAmbientContext() (wrap the parallel task creation code block in this). 

                    Stack Trace:
                    " + Environment.StackTrace;

                    System.Diagnostics.Debug.WriteLine(message);
                }
                else
                {
                    SetAmbientScope(this.parentScope);
                }
            }

            this.disposed = true;
        }

        /*
         * This is where all the magic happens. And there is not much of it.
         * 
         * This implementation is inspired by the source code of the
         * TransactionScope class in .NET 4.5.1 (the TransactionScope class
         * is prior versions of the .NET Fx didn't have support for async
         * operations).
         * 
         * In order to understand this, you'll need to be familiar with the
         * concept of async points. You'll also need to be familiar with the
         * ExecutionContext and CallContext and understand how and why they 
         * flow through async points. Stephen Toub has written an
         * excellent blog post about this - it's a highly recommended read:
         * http://blogs.msdn.com/b/pfxteam/archive/2012/06/15/executioncontext-vs-synchronizationcontext.aspx
         * 
         * Overview: 
         * 
         * We want our DbContextScope instances to be ambient within 
         * the context of a logical flow of execution. This flow may be 
         * synchronous or it may be asynchronous.
         * 
         * If we only wanted to support the synchronous flow scenario, 
         * we could just store our DbContextScope instances in a ThreadStatic 
         * variable. That's the "traditional" (i.e. pre-async) way of implementing
         * an ambient context in .NET. You can see an example implementation of 
         * a TheadStatic-based ambient DbContext here: http://coding.abel.nu/2012/10/make-the-dbcontext-ambient-with-unitofworkscope/ 
         * 
         * But that would be hugely limiting as it would prevent us from being
         * able to use the new async features added to Entity Framework
         * in EF6 and .NET 4.5.
         * 
         * So we need a storage place for our DbContextScope instances 
         * that can flow through async points so that the ambient context is still 
         * available after an await (or any other async point). And this is exactly 
         * what CallContext is for.
         * 
         * There are however two issues with storing our DbContextScope instances 
         * in the CallContext:
         * 
         * 1) Items stored in the CallContext should be serializable. That's because
         * the CallContext flows not just through async points but also through app domain 
         * boundaries. I.e. if you make a remoting call into another app domain, the
         * CallContext will flow through this call (which will require all the values it
         * stores to get serialized) and get restored in the other app domain.
         * 
         * In our case, our DbContextScope instances aren't serializable. And in any case,
         * we most definitely don't want them to be flown accross app domains. So we'll
         * use the trick used by the TransactionScope class to work around this issue.
         * Instead of storing our DbContextScope instances themselves in the CallContext,
         * we'll just generate a unique key for each instance and only store that key in 
         * the CallContext. We'll then store the actual DbContextScope instances in a static
         * Dictionary against their key. 
         * 
         * That way, if an app domain boundary is crossed, the keys will be flown accross
         * but not the DbContextScope instances since a static variable is stored at the 
         * app domain level. The code executing in the other app domain won't see the ambient
         * DbContextScope created in the first app domain and will therefore be able to create
         * their own ambient DbContextScope if necessary.
         * 
         * 2) The CallContext is flow through *all* async points. This means that if someone
         * decides to create multiple threads within the scope of a DbContextScope, our ambient scope
         * will flow through all the threads. Which means that all the threads will see that single 
         * DbContextScope instance as being their ambient DbContext. So clients need to be 
         * careful to always suppress the ambient context before kicking off a parallel operation
         * to avoid our DbContext instances from being accessed from multiple threads.
         * 
         */

        /// <summary>
        /// Makes the provided 'dbContextScope' available as the the ambient scope via the CallContext.
        /// </summary>
        internal static void SetAmbientScope(DbContextScope newAmbientScope)
        {
            if (newAmbientScope == null)
            {
                throw new ArgumentNullException(nameof(newAmbientScope));
            }

            var current = CallContext.LogicalGetData(AmbientDbContextScopeKey) as InstanceIdentifier;
            if (current == newAmbientScope.instanceIdentifier)
            {
                return;
            }

            // Store the new scope's instance identifier in the CallContext, making it the ambient scope
            CallContext.LogicalSetData(AmbientDbContextScopeKey, newAmbientScope.instanceIdentifier);

            // Keep track of this instance (or do nothing if we're already tracking it)
            DbContextScopeInstances.GetValue(newAmbientScope.instanceIdentifier, key => newAmbientScope);
        }

        /// <summary>
        /// Clears the ambient scope from the CallContext and stops tracking its instance. 
        /// Call this when a DbContextScope is being disposed.
        /// </summary>
        internal static void RemoveAmbientScope()
        {
            var current = CallContext.LogicalGetData(AmbientDbContextScopeKey) as InstanceIdentifier;
            CallContext.LogicalSetData(AmbientDbContextScopeKey, null);

            // If there was an ambient scope, we can stop tracking it now
            if (current != null)
            {
                DbContextScopeInstances.Remove(current);
            }
        }

        /// <summary>
        /// Clears the ambient scope from the CallContext but keeps tracking its instance. Call this to temporarily 
        /// hide the ambient context (e.g. to prevent it from being captured by parallel task).
        /// </summary>
        internal static void HideAmbientScope()
        {
            CallContext.LogicalSetData(AmbientDbContextScopeKey, null);
        }

        /// <summary>
        /// Get the current ambient scope or null if no ambient scope has been setup.
        /// </summary>
        internal static DbContextScope GetAmbientScope()
        {
            // Retrieve the identifier of the ambient scope (if any)
            var instanceIdentifier = CallContext.LogicalGetData(AmbientDbContextScopeKey) as InstanceIdentifier;
            if (instanceIdentifier == null)
            {
                return null; // Either no ambient context has been set or we've crossed an app domain boundary and have (intentionally) lost the ambient context
            }

            // Retrieve the DbContextScope instance corresponding to this identifier
            DbContextScope ambientScope;
            if (DbContextScopeInstances.TryGetValue(instanceIdentifier, out ambientScope))
            {
                return ambientScope;
            }

            // We have an instance identifier in the CallContext but no corresponding instance
            // in our DbContextScopeInstances table. This should never happen! The only place where
            // we remove the instance from the DbContextScopeInstances table is in RemoveAmbientScope(),
            // which also removes the instance identifier from the CallContext. 
            //
            // There's only one scenario where this could happen: someone let go of a DbContextScope 
            // instance without disposing it. In that case, the CallContext
            // would still contain a reference to the scope and we'd still have that scope's instance
            // in our DbContextScopeInstances table. But since we use a ConditionalWeakTable to store 
            // our DbContextScope instances and are therefore only holding a weak reference to these instances, 
            // the GC would be able to collect it. Once collected by the GC, our ConditionalWeakTable will return
            // null when queried for that instance. In that case, we're OK. This is a programming error 
            // but our use of a ConditionalWeakTable prevented a leak.
            System.Diagnostics.Debug.WriteLine("Programming error detected. Found a reference to an ambient DbContextScope in the CallContext but didn't have an instance for it in our DbContextScopeInstances table. This most likely means that this DbContextScope instance wasn't disposed of properly. DbContextScope instance must always be disposed. Review the code for any DbContextScope instance used outside of a 'using' block and fix it so that all DbContextScope instances are disposed of.");
            return null;
        }

        private int CommitInternal()
        {
            return this.dbContexts.Commit();
        }

        private Task<int> CommitInternalAsync(CancellationToken cancelToken)
        {
            return this.dbContexts.CommitAsync(cancelToken);
        }

        private void RollbackInternal()
        {
            this.dbContexts.Rollback();
        }
    }
}
