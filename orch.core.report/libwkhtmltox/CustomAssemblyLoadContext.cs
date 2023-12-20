using System.Runtime.Loader;

namespace orch.report.libwkhtmltox
{
    /// <summary>
    /// Represents a custom assembly load context.
    /// </summary>
    internal class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        /// <summary>
        /// Loads an unmanaged library from the specified path.
        /// </summary>
        /// <param name="absolutePath">The absolute path to the unmanaged library.</param>
        /// <returns>A handle to the loaded unmanaged library.</returns>
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            return LoadUnmanagedDll(absolutePath);
        }

        /// <summary>
        /// Loads an unmanaged DLL from the specified path.
        /// </summary>
        /// <param name="unmanagedDllName">The name of the unmanaged DLL to load.</param>
        /// <returns>A handle to the loaded unmanaged DLL.</returns>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return LoadUnmanagedDllFromPath(unmanagedDllName);
        }
    }
}