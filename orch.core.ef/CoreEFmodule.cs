using System.Reflection;

namespace orch.core.ef
{
    public class CoreEFmodule
    {
        public static void Initialize()
        {
            QueryComposer.LoadViews(Assembly.GetExecutingAssembly());
        }
    }
}