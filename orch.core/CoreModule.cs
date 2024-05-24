using System.Reflection;

namespace orch.core
{
    public static class CoreModule
    {
        public const string PERMISSION_GET_SERIALS = "GET_SERIALS";
        public const string PERMISSION_MANAGE_SERIALS = "MANAGE_SERIALS";
        public const string PERMISSION_GET_ROLES = "GET_ROLES";
        public const string PERMISSION_GET_USER = "GET_USER";
        public const string PERMISSION_SYSTEM_ROOT = "SYSTEM_ROOT";
        public const string PERMISSION_CREATE_USER = "CREATE_USER";
        public const string PERMISSION_ASSIGN_ROLE = "ASSIGN_ROLE";
        public const string PERMISSION_MANAGE_ROLES = "MANAGE_ROLES";
        public const string PERMISSION_CHANGE_PASSWORD = "CHANGE_PASSWORD";

        public static void InitializeModule()
        {
            OTransactionService.LoadTTFromAssembly(typeof(CoreModule).Assembly);
            QueryComposer.LoadViews(Assembly.GetExecutingAssembly());
        }

        public static void ResetModule()
        {
            OTransactionService.Reset();
        }

    }
}
