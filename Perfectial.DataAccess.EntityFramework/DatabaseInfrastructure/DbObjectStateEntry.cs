using Perfectial.DataAccess.Interfaces;
using Perfectial.DataAccess.Interfaces.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfectial.DataAccess.EntityFramework
{
    public class DbObjectStateEntry : IDbObjectStateEntry
    {
        public DbObjectState State { get; set; }

        public object EntityKey { get; set; }

        public object Entity { get; set; }
    }
}
