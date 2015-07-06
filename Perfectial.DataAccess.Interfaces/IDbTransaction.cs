using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfectial.DataAccess.Interfaces
{
    public interface IDbTransaction
    {
        void Commit();
        void Rollback();
        void Dispose();
    }
}
