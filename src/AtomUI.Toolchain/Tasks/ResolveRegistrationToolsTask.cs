using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Resolves complete build-tool bundles by identity and content, before analyzer/backend injection.
/// </summary>
public sealed class ResolveRegistrationToolsTask : ITask
{
    /// <summary>
    /// Receives the errors of the task. It is <see langword="null" /> after construction and must be assigned before
    /// <see cref="Execute" /> is called.
    /// </summary>
    public IBuildEngine BuildEngine { get; set; } = null!;
    /// <summary>
    /// Satisfies <see cref="ITask" />. This task never reads it.
    /// </summary>
    public ITaskHost HostObject { get; set; } = null!;
    /// <summary>
    /// The tool assemblies to choose from. Each item's path is the main assembly of a bundle and its <c>Kind</c>
    /// metadata is <c>Generator</c>, <c>Backend</c>, <c>Net8Backend</c> or <c>Tasks</c>. All candidates of a kind
    /// must be the same bundle; the first one in ordinal path order is selected.
    /// </summary>
    public ITaskItem[] Candidates { get; set; } = [];
    /// <summary>
    /// <see langword="true" /> makes the <c>Backend</c> kind required and resolves it: the Mono TypeMap backend
    /// (<c>AtomUI.TypeMap.Linker.dll</c>). While it is <see langword="false" />, <c>Backend</c> candidates are
    /// ignored. The task fails with <c>ATOMUIREG006</c> when it is <see langword="true" /> and no
    /// <c>Backend</c> candidate remains.
    /// </summary>
    public bool IncludeBackend { get; set; }
    /// <summary>
    /// <see langword="true" /> makes the <c>Net8Backend</c> kind required and resolves it: the .NET 8
    /// conditional registration backend (<c>AtomUI.Registration.ILLink8.dll</c>). It follows
    /// <see cref="IncludeBackend" /> in every other respect.
    /// </summary>
    public bool IncludeNet8Backend { get; set; }
    /// <summary>
    /// Path of the selected <c>AtomUI.Generator.dll</c>, or empty when no <c>Generator</c> candidate was given.
    /// </summary>
    [Output] public string GeneratorAssembly { get; set; } = string.Empty;
    /// <summary>
    /// Path of the selected <c>AtomUI.TypeMap.Linker.dll</c>, or empty when no <c>Backend</c> candidate was given.
    /// </summary>
    [Output] public string BackendAssembly { get; set; } = string.Empty;
    /// <summary>
    /// Path of the selected <c>AtomUI.Registration.ILLink8.dll</c>, or empty when no <c>Net8Backend</c> candidate
    /// was given.
    /// </summary>
    [Output] public string Net8BackendAssembly { get; set; } = string.Empty;

    internal static readonly string[] GeneratorFiles = ["AtomUI.Generator.dll", "System.Reflection.Metadata.dll",
        "System.Collections.Immutable.dll", "System.Memory.dll", "System.Buffers.dll", "System.Numerics.Vectors.dll", "System.Runtime.CompilerServices.Unsafe.dll"];
    internal static readonly string[] BackendFiles = ["AtomUI.TypeMap.Linker.dll", "AtomUI.TypeMap.Linker.deps.json",
        "AtomUI.TypeMap.Linker.runtimeconfig.json", "Mono.Cecil.dll"];
    internal static readonly string[] Net8BackendFiles = ["AtomUI.Registration.ILLink8.dll"];
    internal static readonly string[] TaskFiles = ["AtomUI.Build.Tasks.dll", "AtomUI.Build.Tasks.deps.json", "AtomUI.Build.Tasks.runtimeconfig.json", "Microsoft.Build.Framework.dll"];

    /// <summary>
    /// Groups the candidates by kind, checks that each kind is one complete bundle and selects one path per kind.
    /// Candidates of one kind with different identities or file content fail with <c>ATOMUIREG005</c>; an unknown
    /// kind, a wrong file name or a missing bundle file fails with <c>ATOMUIREG006</c>.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when every kind passed and the required backends were resolved; otherwise
    /// <see langword="false" />.
    /// </returns>
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
                throw new InvalidDataException("The required AtomUI Mono TypeMap backend is missing.");
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
