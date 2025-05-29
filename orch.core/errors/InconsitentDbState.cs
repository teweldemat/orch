using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.core.errors
{
    public class InconsistentDbStateException : ApplicationException
    {
        public InconsistentDbStateException(string message) : base(message)
        {

        }
    }
}
