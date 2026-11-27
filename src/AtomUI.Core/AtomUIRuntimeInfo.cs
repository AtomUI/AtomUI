using System.Reflection;

namespace AtomUI;

/// <summary>
/// Exposes the Avalonia and AtomUI versions that this build of AtomUI.Core recorded in its assembly metadata.
/// </summary>
public static class AtomUIRuntimeInfo
{
    /// <summary>
    /// Gets the Avalonia version that AtomUI was built against, or "Unknown" when the metadata is missing.
    /// </summary>
    public static string GetAvaloniaVersion() => GetAssemblyMetadata("AvaloniaVersion");
    /// <summary>
    /// Gets the AtomUI version of this build, or "Unknown" when the metadata is missing.
    /// </summary>
    public static string GetAtomUIVersion() => GetAssemblyMetadata("AtomUIVersion");
    
    private static string GetAssemblyMetadata(string key)
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var attribute in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (attribute.Key == key)
            {
                return attribute.Value ?? "Unknown";
            }
        }

        return "Unknown";
    }
}
