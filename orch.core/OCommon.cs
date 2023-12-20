using Microsoft.EntityFrameworkCore;
using orch.core.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.core
{
    //commonely used routines
    public static class OCommon
    {
        public static void _SaveAndDetach(this DbContext context)
        {
            context.SaveChanges();
            foreach (var e in context.ChangeTracker.Entries())
            {
                e.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
        }
        public static void AssertRootUser(ITransactionDatabase db, Guid? userId,bool allowSystemUser=true)
        {
            var root = db.GetRootUser();
            if (root == null)
                throw new InvalidOperationException("Root user not created");
            
            if (userId == null || root.Id != userId.Value)
                if(!allowSystemUser || userId.Value!=db.GetSystemUserId())
                    throw new UnauthorizedAccessException("Only root user can perform this operation");

        }


    }
}
