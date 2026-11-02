using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

/// <summary>Keep the local authoring bridge available in Editor, outside every player build.</summary>
public sealed class ForgebornePlayerAssemblyFilter : IFilterBuildAssemblies
{
    private static readonly HashSet<string> Removed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public int callbackOrder => 1000;
    internal static void BeginRecord() => Removed.Clear();
    internal static string[] RemovedAssemblyNames => Removed.OrderBy(name => name, StringComparer.Ordinal).ToArray();

    public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
    {
        if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
        return assemblies.Where(path =>
        {
            string name = Path.GetFileName(path);
            bool developmentOnly = string.Equals(name, "MCPForUnity.Runtime.dll", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "MCPForUnity.Editor.dll", StringComparison.OrdinalIgnoreCase);
            if (developmentOnly) Removed.Add(name);
            return !developmentOnly;
        }).ToArray();
    }
}
