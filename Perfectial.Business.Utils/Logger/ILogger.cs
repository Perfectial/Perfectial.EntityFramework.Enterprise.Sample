using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perfectial.Infrastructure.Utils.Logger
{
    public interface ILogger
    {
        bool Warn(string message);

        bool Inform(string message);

        bool Message(string message);
    }
}
