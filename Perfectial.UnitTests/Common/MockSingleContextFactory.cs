using Perfectial.DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfectial.UnitTests.Common
{
    public class MockSingleContextFactory : IDbContextFactory
    {
        private IDbContext _context;
        public MockSingleContextFactory(IDbContext context)
        {
            _context = context;
        }

        public TDbContext CreateDbContext<TDbContext>() where TDbContext : IDbContext
        {
            return (TDbContext)_context;
        }
    }
}
