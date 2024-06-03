using System.Reflection;

namespace orch.core
{
    /// <summary>
    /// Represents a factory that creates an instance of OTransactionService for use within
    /// contexts other than ASP.NET.
    /// </summary>
    public interface IApplicationScopeFactory
    {
        OTransactionService CreateApplicationScope();

        /// <summary>
        /// Loads an implementation of IApplicationScopeFactory from the specified assembly file
        /// </summary>
        /// <returns>An instance of OTransactionService</returns>
        public static OTransactionService LoadFromAssembly(string assemblyFile, string typeName)
        {
            Assembly resolveHandler(object sender, ResolveEventArgs args) => ResolveAssembly(args);
            AppDomain.CurrentDomain.AssemblyResolve += resolveHandler;

            try

            {
                var assembly = Assembly.LoadFrom($@"{assemblyFile}");
                var type = assembly.GetType(typeName);

                if (type == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to load application scope from the specified assembly file ({assemblyFile}): " +
                        $"No implementation of IApplicationScopeFactory was found. " +
                        $"Please ensure that a public implementation of IApplicationScopeFactory is available.");
                }

                var factory = (IApplicationScopeFactory)Activator.CreateInstance(type);
                
                return factory.CreateApplicationScope();
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolveHandler;
            }
        }

        private static Assembly ResolveAssembly(ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name) || args.RequestingAssembly == null)
            {
                return null;
            }

            var assemblyName = new AssemblyName(args.Name);
            var basePath = Path.GetDirectoryName(args.RequestingAssembly.Location);

            if (string.IsNullOrEmpty(basePath))
            {
                return null;
            }

            foreach (var extension in new[] { ".dll", ".exe", ".so" })
            {
                var path = Path.Combine(basePath, assemblyName.Name + extension);
                if (File.Exists(path))
                {
                    try
                    {
                        Console.WriteLine($"Resolving assembly: {assemblyName.Name}");
                        return Assembly.LoadFrom(path);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while resolving assembly {path}: {ex.Message}");
                    }
                }
            }

            return null;
        }
    }
}