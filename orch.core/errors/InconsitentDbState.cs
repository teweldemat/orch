using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.core.errors
{
    public class InconsitentDbStateException:Exception
    {
        public InconsitentDbStateException(string message):base(message)
        {

        }

    }
}
