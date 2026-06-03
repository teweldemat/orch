using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.common
{
    public static class OrchAssert
    {
        public static void NoneNullObject<T>(T? val, string? message = null) where T : class
        {
            if (val == null)
                throw new InvalidOperationException(message == null ? $"Null {typeof(T)} not expected here" : message);

        }
        public static void NoneNullDbObject<T>(T? val, object id, string? message = null) where T : class
        {
            if (val == null)
                throw new InvalidOperationException(message == null ? $"{typeof(T)} with id {id} not in database" : message);
        }
        
    }
}
