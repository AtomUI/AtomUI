using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>Resolves complete build-tool bundles by identity and content, before analyzer/backend injection.</summary>
public sealed class ResolveRegistrationToolsTask : ITask
{
    public IBuildEngine BuildEngine { get; set; } = null!;
    public ITaskHost HostObject { get; set; } = null!;
    public ITaskItem[] Candidates { get; set; } = [];
    public bool IncludeBackend { get; set; }
    public bool IncludeNet8Backend { get; set; }
    [Output] public string GeneratorAssembly { get; set; } = string.Empty;
    [Output] public string BackendAssembly { get; set; } = string.Empty;
    [Output] public string Net8BackendAssembly { get; set; } = string.Empty;

    internal static readonly string[] GeneratorFiles = ["AtomUI.Generator.dll", "System.Reflection.Metadata.dll",
        "System.Collections.Immutable.dll", "System.Memory.dll", "System.Buffers.dll", "System.Numerics.Vectors.dll", "System.Runtime.CompilerServices.Unsafe.dll"];
    internal static readonly string[] BackendFiles = ["AtomUI.TypeMap.Linker.dll", "AtomUI.TypeMap.Linker.deps.json",
        "AtomUI.TypeMap.Linker.runtimeconfig.json", "Mono.Cecil.dll"];
    internal static readonly string[] Net8BackendFiles = ["AtomUI.Registration.ILLink8.dll"];
    internal static readonly string[] TaskFiles = ["AtomUI.Build.Tasks.dll", "AtomUI.Build.Tasks.deps.json", "AtomUI.Build.Tasks.runtimeconfig.json", "Microsoft.Build.Framework.dll"];

    public bool Execute()
    {
        try
        {
            foreach (var group in Candidates.Where(c => (IncludeBackend || c.GetMetadata("Kind") != "Backend") &&
                                                       (IncludeNet8Backend || c.GetMetadata("Kind") != "Net8Backend"))
                         .GroupBy(c => c.GetMetadata("Kind"), StringComparer.Ordinal))
            {
                string? fingerprint = null;
                string? selected = null;
                var files = group.Key switch { "Generator" => GeneratorFiles, "Backend" => BackendFiles, "Net8Backend" => Net8BackendFiles, "Tasks" => TaskFiles,
                    _ => throw new InvalidDataException($"Unknown AtomUI tool kind '{group.Key}'.") };
                foreach (var path in group.Select(c => Path.GetFullPath(c.ItemSpec)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
                {
                    if (Path.GetFileName(path) != files[0])
                    {
                        throw new InvalidDataException($"Unexpected {group.Key} tool '{path}'.");
                    }

                    var current = BundleFingerprint(path, files);
                    if (fingerprint is not null && current != fingerprint)
                    {
                        return Fail("ATOMUIREG005", $"Conflicting {group.Key} build-tool identities or content: '{selected}' and '{path}'. Use one matching AtomUI toolset.");
                    }

                    fingerprint = current;
                    selected ??= path;
                }
                if (group.Key == "Generator")
                {
                    GeneratorAssembly = selected!;
                }

                if (group.Key == "Backend")
                {
                    BackendAssembly = selected!;
                }
                if (group.Key == "Net8Backend")
                {
                    Net8BackendAssembly = selected!;
                }
            }
            if (IncludeBackend && BackendAssembly.Length == 0)
            {
                throw new InvalidDataException("The required AtomUI Browser TypeMap backend is missing.");
            }
            if (IncludeNet8Backend && Net8BackendAssembly.Length == 0)
            {
                throw new InvalidDataException("The required AtomUI .NET 8 conditional registration backend is missing.");
            }

            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or BadImageFormatException or ArgumentException)
        { return Fail("ATOMUIREG006", error.Message); }
    }

    internal static string BundleFingerprint(string main, IEnumerable<string> names)
    {
        var directory = Path.GetDirectoryName(main)!;
        var identity = AssemblyName.GetAssemblyName(main);
        if (identity.Name != Path.GetFileNameWithoutExtension(main))
        {
            throw new InvalidDataException($"Unexpected AtomUI tool assembly identity '{identity}' in '{main}'.");
        }

        var content = new StringBuilder(identity.FullName);
        foreach (var name in names.Order(StringComparer.Ordinal))
        {
            content.Append('\n').Append(name).Append('=').Append(Hash(Path.Combine(directory, name)));
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content.ToString())));
    }
    internal static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
    private bool Fail(string code, string message)
    {
        BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Registration", code, BuildEngine.ProjectFileOfTaskNode, 0, 0, 0, 0, message, null, nameof(ResolveRegistrationToolsTask)));
        return false;
    }
}
