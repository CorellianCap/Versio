#if NETCOREAPP
using System.IO;
using System.Runtime.InteropServices;

namespace Corellian.Versio.Task
{
    // From https://github.com/dotnet/Nerdbank.GitVersioning/blob/03b9f60257e235f1641700bdc9ac64159d91aa1b/src/NerdBank.GitVersioning/LibGit2/LibGit2GitExtensions.cs#L19
    internal static class LibGit2GitExtensions
    {
        public static string? FindLibGit2NativeBinaries(string basePath)
        {
            string arch = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

            // TODO: learn how to detect when to use "linux-musl".
            string? os =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" :
                null;

            if (os is null)
            {
                return null;
            }

            string candidatePath = Path.Combine(basePath, "runtimes", $"{os}-{arch}", "native");
            return Directory.Exists(candidatePath) ? candidatePath : null;
        }
    }
}
#endif