using Perfectial.DataAccess.Interfaces.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfectial.DataAccess.Interfaces
{
    public interface IDbObjectStateEntry
    {
        DbObjectState State { get; set; }

        object EntityKey { get; set; }

        object Entity { get; set; }
    }
}
