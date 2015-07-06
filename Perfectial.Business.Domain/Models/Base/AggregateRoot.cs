using Perfectial.Core.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Perfectial.Core.Domain.Models.Base
{
    public abstract class AggregateRoot<TEntity> : BaseEntity
        where TEntity : AggregateRoot<TEntity>
    {
        public abstract List<Expression<Func<TEntity, object>>> GetRootMeassures();
    }
}
