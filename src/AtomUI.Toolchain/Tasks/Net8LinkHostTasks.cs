using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using AtomUI.Build.Tasks.Isolation;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>Freezes the pinned official ILLink8 tools with an explicit, isolated .NET 10 host configuration.</summary>
public sealed class PrepareNet8LinkHostTask : RegistrationBuildTask
{
    private const string LinkerVersion = "8.0.27+a6bde67c455f2ac219988c7a66171631090b6f65";
    private const string HostConfig = "{\"runtimeOptions\":{\"tfm\":\"net10.0\",\"rollForward\":\"LatestPatch\",\"framework\":{\"name\":\"Microsoft.NETCore.App\",\"version\":\"10.0.0\"},\"configProperties\":{\"System.GC.Server\":true}}}";
    [Required] public string LinkerTaskAssembly { get; set; } = string.Empty;
    [Required] public string BackendAssembly { get; set; } = string.Empty;
    [Required] public string OverrideSourceTemplate { get; set; } = string.Empty;
    public string OverrideSourceText { get; set; } = string.Empty;
    [Required] public string OutputRoot { get; set; } = string.Empty;
    [Output] public string LinkerPath { get; set; } = string.Empty;
    [Output] public string TaskOverrideSource { get; set; } = string.Empty;
    [Output] public string HostFingerprint { get; set; } = string.Empty;
    [Output] public ITaskItem[] HostInputs { get; set; } = [];

    public override bool Execute()
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(OutputRoot);
            var taskPath = Path.GetFullPath(LinkerTaskAssembly);
            var directory = Path.GetDirectoryName(taskPath)!;
            var linker = Path.Combine(directory, "illink.dll");
            if (Path.GetFileName(taskPath) != "ILLink.Tasks.dll" ||
                AssemblyName.GetAssemblyName(linker).FullName != "illink, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" ||
                TypeMapBuildContract.InformationalVersion(linker) != LinkerVersion ||
                TypeMapBuildContract.InformationalVersion(taskPath) != LinkerVersion)
            {
                throw new InvalidDataException("Controlled registration requires the official ILLink Tasks and engine 8.0.27 from the same tool directory.");
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
            var state = JsonSerializer.Serialize(new { format = 1, linkerVersion = LinkerVersion, files = records });
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
            .Where(field => reader.GetString(field.Name) == "LinkerProductVersion" && !field.GetDefaultValue().IsNil)
            .Select(field => reader.GetConstant(field.GetDefaultValue())).ToArray();
        if (constants.Length != 1 || constants[0].TypeCode != ConstantTypeCode.String ||
            Encoding.Unicode.GetString(reader.GetBlobBytes(constants[0].Value)) != LinkerVersion)
        {
            throw new InvalidDataException("Conditional backend does not declare the required ILLink8 capability.");
        }
    }

}
