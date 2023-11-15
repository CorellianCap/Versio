#if NETCOREAPP
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Corellian.Versio.Task
{
    // From https://github.com/dotnet/Nerdbank.GitVersioning/blob/9631e3504847614443080f80d17312d81869e171/src/Nerdbank.GitVersioning.Tasks/GitLoaderContext.cs
    public class GitLoaderContext : AssemblyLoadContext
    {
        public const string RuntimePath = "./runtimes";
        private readonly string nativeDependencyBasePath;

        private (string?, IntPtr) lastLoadedLibrary;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitLoaderContext"/> class.
        /// </summary>
        /// <param name="nativeDependencyBasePath">The path to the directory that contains the "runtimes" folder.</param>
        public GitLoaderContext(string nativeDependencyBasePath)
        {
            this.nativeDependencyBasePath = nativeDependencyBasePath;
        }

        /// <inheritdoc/>
        protected override Assembly Load(AssemblyName assemblyName)
        {
            string path = Path.Combine(Path.GetDirectoryName(typeof(GitLoaderContext).Assembly.Location)!, assemblyName.Name + ".dll");
            return File.Exists(path)
                ? this.LoadFromAssemblyPath(path)
                : Default.LoadFromAssemblyName(assemblyName);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            IntPtr p = base.LoadUnmanagedDll(unmanagedDllName);

            if (p == IntPtr.Zero)
            {
                if (unmanagedDllName == this.lastLoadedLibrary.Item1)
                {
                    return this.lastLoadedLibrary.Item2;
                }

                string prefix =
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? string.Empty :
                    "lib";

                string? extension =
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ".so" :
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" :
                    null;

                string fileName = $"{prefix}{unmanagedDllName}{extension}";
                string? directoryPath = LibGit2GitExtensions.FindLibGit2NativeBinaries(this.nativeDependencyBasePath);
                if (directoryPath is not null && NativeLibrary.TryLoad(Path.Combine(directoryPath, fileName), out p))
                {
                    // Cache this to make us a little faster next time.
                    this.lastLoadedLibrary = (unmanagedDllName, p);
                }
            }

            return p;
        }
    }
}
#endif