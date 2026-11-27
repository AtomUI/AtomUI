using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.RegularExpressions;
using AtomUI.Build.Tasks.Isolation;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Freezes final SDK implementation inputs and returns the paths the engine must consume.
/// </summary>
public sealed class PrepareConditionalRegistrationInputsTask : RegistrationBuildTask
{
    /// <summary>
    /// Directory under which the input snapshot is created. Every later task must read the snapshot paths this
    /// task returns instead of the original input paths.
    /// </summary>
    [Required] public string OutputRoot { get; set; } = string.Empty;
    /// <summary>
    /// Configuration of this build. The snapshot records it together with <see cref="EngineArguments" /> and the
    /// item metadata of the inputs.
    /// </summary>
    [Required] public string Configuration { get; set; } = string.Empty;
    /// <summary>
    /// The implementation assemblies to freeze, each item spec a managed assembly path. Exactly one
    /// <c>AtomUI.Core</c> identity must be present. The item metadata is recorded with the snapshot and kept on
    /// <see cref="PreparedAssemblies" />.
    /// </summary>
    public ITaskItem[] Assemblies { get; set; } = [];
    /// <summary>
    /// Inputs other than assemblies. The <c>Kind</c> metadata names the role, for example <c>tool</c>,
    /// <c>custom-step</c> or <c>opaque</c>; a missing or blank kind means <c>configuration</c>.
    /// An <c>opaque</c> item must declare <c>CustomDataKey</c> and <c>Value</c>, with the value being its own path,
    /// and one key cannot cover two files. An item whose <c>EngineOption</c> is <c>link-attributes</c> must appear
    /// in <see cref="EngineArguments" /> as <c>--link-attributes</c> arguments exactly once.
    /// </summary>
    public ITaskItem[] AdditionalInputs { get; set; } = [];
    /// <summary>
    /// The linker arguments the SDK produced. Raw arguments that name files (response files, descriptors,
    /// references, custom steps, custom data or substitutions) are rejected as mutable; such inputs must come in as
    /// <see cref="Assemblies" /> or <see cref="AdditionalInputs" /> items. The snapshot records the arguments with
    /// <see cref="Configuration" />.
    /// </summary>
    public string EngineArguments { get; set; } = string.Empty;
    /// <summary>
    /// Path of the input list the engine must read. It is written into a fresh run directory under the snapshot and
    /// names the frozen assemblies, the frozen additional inputs and the analysis report the engine has to write.
    /// Empty on failure.
    /// </summary>
    [Output] public string InputsPath { get; set; } = string.Empty;
    /// <summary>
    /// Name of the snapshot directory under <see cref="OutputRoot" /> that holds the frozen inputs. It identifies
    /// the frozen input set. Empty on failure.
    /// </summary>
    [Output] public string InputFingerprint { get; set; } = string.Empty;
    /// <summary>
    /// <see cref="Assemblies" /> with each item pointing at its frozen copy: item spec and full path are the
    /// snapshot path, and the <c>AtomUIOriginalPath</c> metadata keeps the original path. Empty on failure.
    /// </summary>
    [Output] public ITaskItem[] PreparedAssemblies { get; set; } = [];
    /// <summary>
    /// <see cref="AdditionalInputs" /> with each item pointing at its frozen copy. The <c>AtomUIOriginalPath</c>
    /// and <c>AtomUIOriginalItemSpec</c> metadata keep the original location; <c>Kind</c> is set explicitly.
    /// Empty on failure.
    /// </summary>
    [Output] public ITaskItem[] PreparedAdditionalInputs { get; set; } = [];
    /// <summary>
    /// <see cref="EngineArguments" /> with the <c>link-attributes</c> arguments the SDK produced removed and
    /// re-created from the frozen link-attributes items, so the linker reads only frozen copies. Empty on failure.
    /// </summary>
    [Output] public string PreparedEngineArguments { get; set; } = string.Empty;

    /// <summary>
    /// Freezes the inputs under <see cref="OutputRoot" /> and publishes the paths the engine must consume.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the snapshot is in place and the outputs are set. When an input is invalid, or
    /// changed while it was frozen, the task logs the error as <c>ATOMUIREG006</c>, clears every output, deletes
    /// the run directory it created and returns <see langword="false" />.
    /// </returns>
    public override bool Execute()
    {
        string? executionDirectory = null;
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(OutputRoot);
            var suppressionInputs = AdditionalInputs.Where(item => item.GetMetadata("EngineOption") == "link-attributes").ToArray();
            var engineArguments = EngineArguments;
            if (suppressionInputs.Length != 0)
            {
                var sdkArguments = LinkAttributeArguments(suppressionInputs.Select(item => item.ItemSpec));
                var offset = engineArguments.IndexOf(sdkArguments, StringComparison.Ordinal);
                if (offset < 0 || offset != engineArguments.LastIndexOf(sdkArguments, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("SDK link-attributes arguments cannot be bound uniquely to the declared _ILLinkSuppressions items.");
                }
                engineArguments = engineArguments.Remove(offset, sdkArguments.Length);
            }
            if (Regex.IsMatch(engineArguments.Replace('"', ' ').Replace('\'', ' '),
                    @"(?:^|\s)(?:@|(?:--substitutions|--link-attributes|--custom-step|--custom-data|--descriptor|--reference|--root-assembly|-reference|-x|-a|-d)(?:\s|=|$))", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
            {
                throw new InvalidDataException("Raw ILLink file/response/custom-step arguments are not immutable inputs. Use TrimmerRootDescriptor, ReferencePath, structured custom steps with AtomUIBundleRoot, or declared CustomDataKey opaque inputs.");
            }
            var inputs = Assemblies.Select(item => new
            {
                Item = item, Identity = AssemblyName.GetAssemblyName(item.ItemSpec)
            }).ToArray();
            var cores = inputs.Where(input => input.Identity.Name == "AtomUI.Core")
                .Select(input => input.Identity.FullName!).Distinct(StringComparer.Ordinal).ToArray();
            if (cores.Length != 1)
            {
                throw new InvalidDataException("Conditional registration requires exactly one resolved AtomUI.Core implementation identity.");
            }
            var additional = AdditionalInputs.Select(item =>
            {
                var kind = string.IsNullOrWhiteSpace(item.GetMetadata("Kind")) ? "configuration" : item.GetMetadata("Kind");
                if (kind == "opaque" && (string.IsNullOrWhiteSpace(item.GetMetadata("CustomDataKey")) ||
                    item.GetMetadata("CustomDataKey").StartsWith("AtomUIControlled", StringComparison.Ordinal) || item.GetMetadata("CustomDataKey") == "AtomUIConditionalInputs" ||
                    string.IsNullOrWhiteSpace(item.GetMetadata("Value")) ||
                    Path.GetFullPath(item.GetMetadata("Value")) != Path.GetFullPath(item.ItemSpec)))
                {
                    throw new InvalidDataException($"Opaque input '{item.ItemSpec}' must declare CustomDataKey matching an existing custom-data value containing exactly that file path.");
                }
                return (Item: item, Kind: kind, Path: Path.GetFullPath(item.ItemSpec));
            }).ToArray();
            if (additional.Where(input => input.Kind == "opaque").GroupBy(input => input.Item.GetMetadata("CustomDataKey"), StringComparer.Ordinal)
                .Any(group => group.Select(input => input.Path).Distinct(StringComparer.Ordinal).Count() != 1))
            {
                throw new InvalidDataException("An opaque custom-data key cannot map to multiple input files.");
            }
            // Per-assembly trim actions and other SDK input metadata affect marking even when the
            // assembly bytes do not change. They are part of the configuration, not just diagnostics.
            var profile = JsonSerializer.Serialize(new
            {
                Configuration, EngineArguments,
                assemblies = inputs.OrderBy(input => input.Identity.FullName, StringComparer.Ordinal).Select(input => new
                {
                    identity = input.Identity.FullName,
                    metadata = TaskWireItem.FromItem(input.Item).Metadata.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                }).ToArray(),
                additional = additional.Select(input => new { input.Path, input.Kind,
                    metadata = TaskWireItem.FromItem(input.Item).Metadata.OrderBy(pair => pair.Key, StringComparer.Ordinal) }).ToArray()
            });
            var manifestPath = ConditionalRegistrationSnapshot.PrepareInputs(OutputRoot, Assemblies.Select(item => item.ItemSpec).ToArray(),
                cores[0], profile, additional.Select(input => new ConditionalAdditionalInput(input.Path, input.Kind,
                    input.Item.GetMetadata("AtomUIBundleRoot"))).ToArray());
            using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var frozen = manifest.RootElement.GetProperty("assemblies").EnumerateArray()
                .ToDictionary(item => item.GetProperty("assemblyIdentity").GetString()!, item =>
                    (Path: item.GetProperty("path").GetString()!, Hash: item.GetProperty("sha256").GetString()!), StringComparer.Ordinal);
            var prepared = new List<ITaskItem>();
            foreach (var input in inputs)
            {
                var current = ConditionalRegistrationMetadata.Read(input.Item.ItemSpec, cores[0]);
                if (!frozen.TryGetValue(current.AssemblyIdentity, out var snapshot) || current.ContentHash != snapshot.Hash)
                {
                    throw new InvalidDataException("Implementation inputs changed while preparing the registration snapshot; rebuild before publishing.");
                }
                var item = TaskWireItem.FromItem(input.Item);
                item.SetMetadata("AtomUIOriginalPath", Path.GetFullPath(item.ItemSpec));
                item.ItemSpec = snapshot.Path;
                item.FullPath = snapshot.Path;
                prepared.Add(item);
            }
            PreparedAssemblies = prepared.ToArray();
            var cacheDirectory = Path.GetDirectoryName(manifestPath)!;
            using var state = JsonDocument.Parse(File.ReadAllText(Path.Combine(cacheDirectory, "input-state.json")));
            var additionalMap = state.RootElement.GetProperty("inputs").EnumerateArray()
                .Where(item => item.GetProperty("Kind").GetString() != "assemblies")
                .ToDictionary(item => (Path: item.GetProperty("SourcePath").GetString()!, Kind: item.GetProperty("Kind").GetString()!),
                    item => Path.Combine(cacheDirectory, item.GetProperty("RelativePath").GetString()!));
            var preparedAdditional = new List<ITaskItem>();
            foreach (var input in additional)
            {
                var item = TaskWireItem.FromItem(input.Item);
                item.SetMetadata("AtomUIOriginalPath", input.Path);
                item.SetMetadata("AtomUIOriginalItemSpec", input.Item.ItemSpec);
                item.SetMetadata("Kind", input.Kind);
                item.ItemSpec = additionalMap[(input.Path, input.Kind)];
                item.FullPath = item.ItemSpec;
                preparedAdditional.Add(item);
            }
            ValidateCustomStepDependencies(preparedAdditional);
            PreparedAdditionalInputs = preparedAdditional.ToArray();
            PreparedEngineArguments = engineArguments + LinkAttributeArguments(preparedAdditional.Where(item =>
                item.GetMetadata("EngineOption") == "link-attributes").Select(item => item.ItemSpec));
            InputFingerprint = Path.GetFileName(cacheDirectory);
            // Immutable assemblies can be shared. Mutable analysis/linked outputs cannot: two SDK
            // publishes with identical inputs must not overwrite each other's report or semaphore.
            executionDirectory = Path.Combine(cacheDirectory, "runs", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(executionDirectory);
            InputsPath = Path.Combine(executionDirectory, "inputs.json");
            File.WriteAllText(InputsPath, JsonSerializer.Serialize(new
            {
                format = 2, coreAssemblyIdentity = cores[0],
                assemblies = manifest.RootElement.GetProperty("assemblies"),
                additionalInputs = manifest.RootElement.GetProperty("additionalInputs"),
                analysisReportPath = Path.Combine(executionDirectory, "analysis.json")
            }));
            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or BadImageFormatException)
        {
            InputsPath = string.Empty;
            InputFingerprint = string.Empty;
            PreparedAssemblies = [];
            PreparedAdditionalInputs = [];
            PreparedEngineArguments = string.Empty;
            if (executionDirectory is not null && Directory.Exists(executionDirectory))
            {
                Directory.Delete(executionDirectory, recursive: true);
            }
            return Fail("ATOMUIREG006", error);
        }
    }

    private static string LinkAttributeArguments(IEnumerable<string> paths)
    {
        var values = paths.ToArray();
        if (values.Any(path => path.IndexOfAny(['"', '\r', '\n']) >= 0))
        {
            throw new InvalidDataException("Link-attributes file paths cannot contain quotes or line breaks.");
        }
        return values.Length == 0 ? string.Empty : " --link-attributes \"" + string.Join("\" --link-attributes \"", values) + "\"";
    }

    private static void ValidateCustomStepDependencies(IReadOnlyList<ITaskItem> inputs)
    {
        var steps = inputs.Where(item => item.GetMetadata("Kind") == "custom-step").ToArray();
        if (steps.Length == 0)
        {
            return;
        }
        var framework = new Dictionary<string, AssemblyName>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.GetFiles(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"))
        {
            try
            {
                var name = AssemblyName.GetAssemblyName(path);
                framework[name.Name!] = name;
            }
            catch (BadImageFormatException) { } // Native runtime binaries are not managed dependencies.
        }
        var toolIdentities = inputs.Where(item => item.GetMetadata("Kind") == "tool" &&
                Path.GetFileName(item.ItemSpec) is "illink.dll" or "Mono.Cecil.dll" or "Mono.Cecil.Rocks.dll" or "Mono.Cecil.Pdb.dll" or "Mono.Cecil.Mdb.dll")
            .Select(item => AssemblyName.GetAssemblyName(item.ItemSpec).FullName!).ToHashSet(StringComparer.Ordinal);
        foreach (var step in steps)
        {
            // A declared closed bundle is required; normal runtime/framework and ILLink APIs are
            // supplied by the controlled host, but other assembly dependencies must travel with it.
            var relative = Path.GetRelativePath(Path.GetFullPath(step.GetMetadata("AtomUIBundleRoot")), step.GetMetadata("AtomUIOriginalPath"));
            var bundleRoot = step.ItemSpec;
            foreach (var _ in relative.Split(Path.DirectorySeparatorChar))
            {
                bundleRoot = Path.GetDirectoryName(bundleRoot)!;
            }
            var definitions = new HashSet<string>(StringComparer.Ordinal);
            var managedFiles = new List<string>();
            foreach (var path in Directory.GetFiles(bundleRoot, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    definitions.Add(AssemblyName.GetAssemblyName(path).FullName!);
                    managedFiles.Add(path);
                }
                catch (BadImageFormatException) { } // A declared bundle may also contain native DLLs.
            }
            foreach (var path in managedFiles)
            {
                using var stream = File.OpenRead(path);
                using var pe = new PEReader(stream);
                var reader = pe.GetMetadataReader();
                foreach (var handle in reader.AssemblyReferences)
                {
                    var identity = ConditionalRegistrationMetadata.GetIdentity(reader, reader.GetAssemblyReference(handle));
                    var name = new AssemblyName(identity);
                    if (definitions.Contains(identity) || toolIdentities.Contains(identity) ||
                        (framework.TryGetValue(name.Name!, out var runtime) &&
                         (runtime.GetPublicKeyToken() ?? []).SequenceEqual(name.GetPublicKeyToken() ?? [])))
                    {
                        continue;
                    }
                    throw new InvalidDataException($"Custom step '{step.GetMetadata("AtomUIOriginalPath")}' depends on '{identity}' outside AtomUIBundleRoot. Declare a complete adjacent dependency/configuration bundle; opaque file paths require declared CustomDataKey mappings.");
                }
            }
        }
    }
}
