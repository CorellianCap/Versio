using System;
using System.IO;
using System.Linq;
using System.Reflection;
#if NETCOREAPP
using System.Runtime.Loader;
using Microsoft.Build.Framework;
#endif

namespace Corellian.Versio.Task
{
    // From https://github.com/matkoch/custom-msbuild-task/blob/94fe4f658777dc01a35078ea6933c440fe92cc5e/src/CustomTasks/ContextAwareTask.cs
    public abstract class ContextAwareTask : Microsoft.Build.Utilities.Task
    {
        protected virtual string ManagedDllDirectory => Path.GetDirectoryName(new Uri(GetType().GetTypeInfo().Assembly.Location).LocalPath);

        protected virtual string UnmanagedDllDirectory => null;

        public sealed override bool Execute()
        {
#if NETCOREAPP
        var taskAssemblyPath = new Uri(GetType().GetTypeInfo().Assembly.Location).LocalPath;
        var context = new GitLoaderContext(ManagedDllDirectory);
        var inContextAssembly = context.LoadFromAssemblyPath(taskAssemblyPath);
        var innerTaskType = inContextAssembly.GetType(GetType().FullName);
        var innerTask = Activator.CreateInstance(innerTaskType);

        var outerProperties = GetType().GetRuntimeProperties().ToDictionary(i => i.Name);
        var innerProperties = innerTaskType.GetRuntimeProperties().ToDictionary(i => i.Name);
        var propertiesDiscovery =
            from outerProperty in outerProperties.Values
            where outerProperty.SetMethod != null && outerProperty.GetMethod != null
            let innerProperty = innerProperties[outerProperty.Name]
            select new { outerProperty, innerProperty };

        var propertiesMap = propertiesDiscovery.ToArray();
        var outputPropertiesMap = propertiesMap.Where(pair => pair.outerProperty.GetCustomAttribute<OutputAttribute>() != null).ToArray();

        foreach (var propertyPair in propertiesMap)
        {
            var outerPropertyValue = propertyPair.outerProperty.GetValue(this);
            propertyPair.innerProperty.SetValue(innerTask, outerPropertyValue);
        }

        var executeInnerMethod = innerTaskType.GetMethod(nameof(ExecuteInner), BindingFlags.Instance | BindingFlags.NonPublic);
        var result = (bool)executeInnerMethod.Invoke(innerTask, new object[0]);

        foreach (var propertyPair in outputPropertiesMap)
            propertyPair.outerProperty.SetValue(this, propertyPair.innerProperty.GetValue(innerTask));

        return result;
#else
            // On .NET Framework (on Windows), we find native binaries by adding them to our PATH.
            if (UnmanagedDllDirectory != null)
            {
                var pathEnvVar = Environment.GetEnvironmentVariable("PATH");
                var searchPaths = pathEnvVar.Split(Path.PathSeparator);
                if (!searchPaths.Contains(UnmanagedDllDirectory, StringComparer.OrdinalIgnoreCase))
                {
                    pathEnvVar += Path.PathSeparator + UnmanagedDllDirectory;
                    Environment.SetEnvironmentVariable("PATH", pathEnvVar);
                }
            }

            return ExecuteInner();
#endif
        }

        protected abstract bool ExecuteInner();

    }
}
