using System.Reflection;
using System.Runtime.InteropServices;

namespace orch.report.libwkhtmltox
{
	internal static class DinkToPdfLibrary
	{
		/// <summary>
		/// Loads the platform-specific libwkhtmltox library required by DinkToPdf.
		/// </summary>
		internal static void Load()
		{
			string libraryName = "libwkhtmltox";
			string libraryExtension;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				libraryExtension = ".dll";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				libraryExtension = ".so";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				libraryExtension = ".dylib";
			}
			else
			{
				throw new PlatformNotSupportedException("Unsupported platform");
			}

			var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			if (basePath != null)
			{
				string fullPath = Path.Combine(basePath, libraryName, libraryName + libraryExtension);
				(new CustomAssemblyLoadContext()).LoadUnmanagedLibrary(fullPath);
			}
		}
	}
}