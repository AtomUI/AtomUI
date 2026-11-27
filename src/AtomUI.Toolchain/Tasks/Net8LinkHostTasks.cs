using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using AtomUI.Build.Tasks.Isolation;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Freezes the SDK-selected official ILLink8 tools with an explicit, isolated .NET 10+ host configuration.
/// </summary>
public sealed class PrepareNet8LinkHostTask : RegistrationBuildTask
{
    internal const string HostConfig = "{\"runtimeOptions\":{\"tfm\":\"net10.0\",\"rollForward\":\"Major\",\"framework\":{\"name\":\"Microsoft.NETCore.App\",\"version\":\"10.0.0\"},\"configProperties\":{\"System.GC.Server\":true}}}";
    /// <summary>
    /// Path of the official <c>ILLink.Tasks.dll</c>. Its directory is the tool directory: <c>illink.dll</c> must
    /// sit beside it, and both must be the official ILLink 8.x tasks and engine from one servicing release.
    /// </summary>
    [Required] public string LinkerTaskAssembly { get; set; } = string.Empty;
    /// <summary>
    /// Path of the <c>AtomUI.Registration.ILLink8.dll</c> adapter, which must declare the ILLink8 linker
    /// capability. It is frozen into the host as <c>adapter/AtomUI.Registration.ILLink8.dll</c>.
    /// </summary>
    [Required] public string BackendAssembly { get; set; } = string.Empty;
    /// <summary>
    /// Path of the template file that is frozen into the host as <c>AtomUI.Net8.ILLink.Task.cs</c>. It is read
    /// only when <see cref="OverrideSourceText" /> is empty.
    /// </summary>
    [Required] public string OverrideSourceTemplate { get; set; } = string.Empty;
    /// <summary>
    /// Text that replaces the <see cref="OverrideSourceTemplate" /> file as the content of the frozen
    /// <c>AtomUI.Net8.ILLink.Task.cs</c>. Empty means read the template file.
    /// </summary>
    public string OverrideSourceText { get; set; } = string.Empty;
    /// <summary>
    /// Root directory for the frozen host. The host is materialized in the subdirectory named by
    /// <see cref="HostFingerprint" />.
    /// </summary>
    [Required] public string OutputRoot { get; set; } = string.Empty;
    /// <summary>
    /// Path of the frozen <c>illink.dll</c> under <see cref="OutputRoot" />. Empty when the task failed.
    /// </summary>
    [Output] public string LinkerPath { get; set; } = string.Empty;
    /// <summary>
    /// Path of the frozen <c>AtomUI.Net8.ILLink.Task.cs</c> file. Empty when the task failed.
    /// </summary>
    [Output] public string TaskOverrideSource { get; set; } = string.Empty;
    /// <summary>
    /// SHA-256 hash of the host state (the linker version and the hash of every frozen file) that names the host
    /// directory under <see cref="OutputRoot" />. Empty when the task failed.
    /// </summary>
    [Output] public string HostFingerprint { get; set; } = string.Empty;
    /// <summary>
    /// The frozen files as items, each with <c>FullPath</c> set, followed by <c>host-state.json</c>.
    /// Empty when the task failed.
    /// </summary>
    [Output] public ITaskItem[] HostInputs { get; set; } = [];

    /// <summary>
    /// Validates the ILLink tools and the backend, freezes them with the task source and the host configuration
    /// under <see cref="OutputRoot" />, and sets the outputs. Returns <see langword="true" /> when the frozen
    /// host is ready. Otherwise it logs error <c>ATOMUIREG006</c>, clears the outputs and returns
    /// <see langword="false" />.
    /// </summary>
    public override bool Execute()
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(OutputRoot);
            var taskPath = Path.GetFullPath(LinkerTaskAssembly);
            var directory = Path.GetDirectoryName(taskPath)!;
            var linker = Path.Combine(directory, "illink.dll");
            var linkerVersion = TypeMapBuildContract.InformationalVersion(linker);
            var tasksVersion = TypeMapBuildContract.InformationalVersion(taskPath);
            if (Path.GetFileName(taskPath) != "ILLink.Tasks.dll" ||
                !Net8EngineToolchain.IsSupportedLinker(AssemblyName.GetAssemblyName(linker).FullName, linkerVersion) ||
                !Net8EngineToolchain.IsSameEngine(tasksVersion, linkerVersion))
            {
                throw new InvalidDataException($"Controlled registration requires official ILLink Tasks and engine 8.x from one servicing release; found {tasksVersion} / {linkerVersion}.");
            }
            ValidateBackend(BackendAssembly);
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal).ToDictionary(path => "official/" + Path.GetRelativePath(directory, path).Replace('\\', '/'),
                    File.ReadAllBytes, StringComparer.Ordinal);
            foreach (var required in new[] { "illink.dll", "illink.deps.json", "illink.runtimeconfig.json", "Mono.Cecil.dll", "ILLink.Tasks.dll" })
            {
                if (!files.ContainsKey("official/" + required))
                {
                    throw new InvalidDataException("Incomplete official ILLink8 tool bundle: " + required);
                }
            }
            files.Add("adapter/AtomUI.Registration.ILLink8.dll", File.ReadAllBytes(BackendAssembly));
            files.Add("AtomUI.Net8.ILLink.Task.cs", string.IsNullOrEmpty(OverrideSourceText)
                ? File.ReadAllBytes(OverrideSourceTemplate) : Encoding.UTF8.GetBytes(OverrideSourceText));
            files.Add("atomui.net8-linkhost.runtimeconfig.json", Encoding.UTF8.GetBytes(HostConfig));
            var records = files.OrderBy(file => file.Key, StringComparer.Ordinal).Select(file =>
                new { path = file.Key, sha256 = Hash(file.Value) }).ToArray();
            var state = JsonSerializer.Serialize(new { format = 1, linkerVersion, files = records });
            var fingerprint = Hash(Encoding.UTF8.GetBytes(state));
            var root = Path.GetFullPath(OutputRoot);
            using var ownership = ToolBundleCache.Acquire(Path.Combine(root, fingerprint + ".lock"), TimeSpan.FromSeconds(30), 25);
            var destination = Path.Combine(root, fingerprint);
            files.Add("host-state.json", Encoding.UTF8.GetBytes(state));
            ToolBundleCache.Prepare(destination, files, "Prepared ILLink8 host was changed or is incomplete; refusing stale tool reuse.");
            LinkerPath = Path.Combine(destination, "official", "illink.dll");
            TaskOverrideSource = Path.Combine(destination, "AtomUI.Net8.ILLink.Task.cs");
            HostFingerprint = fingerprint;
            HostInputs = records.Select(record => (ITaskItem)new TaskWireItem
            {
                ItemSpec = Path.Combine(destination, record.path), FullPath = Path.Combine(destination, record.path)
            }).Append(new TaskWireItem { ItemSpec = Path.Combine(destination, "host-state.json") }).ToArray();
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or BadImageFormatException or InvalidDataException)
        {
            LinkerPath = TaskOverrideSource = HostFingerprint = string.Empty;
            HostInputs = [];
            return Fail("ATOMUIREG006", error);
        }
    }

    private static void ValidateBackend(string path)
    {
        if (AssemblyName.GetAssemblyName(path).Name != "AtomUI.Registration.ILLink8")
        {
            throw new InvalidDataException("Conditional ILLink8 backend has an unknown identity.");
        }
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var reader = pe.GetMetadataReader();
        var constants = reader.TypeDefinitions.Select(reader.GetTypeDefinition)
            .Where(type => reader.GetString(type.Namespace) == "AtomUI.Registration.ILLink8" && reader.GetString(type.Name) == "ConditionalRegistrationBackend")
            .SelectMany(type => type.GetFields()).Select(reader.GetFieldDefinition)
            .Where(field => reader.GetString(field.Name) == "LinkerCapability" && !field.GetDefaultValue().IsNil)
            .Select(field => reader.GetConstant(field.GetDefaultValue())).ToArray();
        if (constants.Length != 1 || constants[0].TypeCode != ConstantTypeCode.String ||
            Encoding.Unicode.GetString(reader.GetBlobBytes(constants[0].Value)) != Net8EngineToolchain.BackendCapability)
        {
            throw new InvalidDataException("Conditional backend does not declare the required ILLink8 capability.");
        }
    }

}
