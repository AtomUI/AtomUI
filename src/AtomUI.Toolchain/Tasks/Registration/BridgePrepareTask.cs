using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AtomUI.Build.Tasks.Isolation;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>Prepares an isolated official-TypeMap input set without loading consumer assemblies.</summary>
public sealed class PrepareConditionalRegistrationBridgeTask : RegistrationBuildTask
{
    [Required] public string OutputRoot { get; set; } = string.Empty;
    [Required] public string Configuration { get; set; } = string.Empty;
    public ITaskItem[] Assemblies { get; set; } = [];
    public ITaskItem[] AdditionalInputs { get; set; } = [];
    public string EngineArguments { get; set; } = string.Empty;
    [Required] public string BridgeAssembly { get; set; } = string.Empty;
    [Required] public string CecilAssembly { get; set; } = string.Empty;
    [Required] public string RuntimePackDirectory { get; set; } = string.Empty;
    [Required] public string ApplicationAssemblyIdentity { get; set; } = string.Empty;
    [Required] public string DotNetExecutable { get; set; } = string.Empty;
    public bool ProbeOnly { get; set; }
    [Output] public bool NeedsBridge { get; set; }
    [Output] public string InputsPath { get; set; } = string.Empty;
    [Output] public string TransformationPath { get; set; } = string.Empty;
    [Output] public ITaskItem[] PreparedAssemblies { get; set; } = [];
    [Output] public ITaskItem[] PreparedAdditionalInputs { get; set; } = [];
    [Output] public string PreparedEngineArguments { get; set; } = string.Empty;

    public override bool Execute()
    {
        try
        {
            NeedsBridge = BridgeInputProbe.RequiresBridge(Assemblies);
            if (ProbeOnly || !NeedsBridge)
            {
                PreparedAssemblies = Assemblies;
                PreparedAdditionalInputs = AdditionalInputs;
                PreparedEngineArguments = EngineArguments;
                return true;
            }
            var hostDirectory = PrepareHost();
            var additional = AdditionalInputs.Select(TaskWireItem.FromItem).Cast<ITaskItem>().ToList();
            var engineArguments = EngineArguments;
            foreach (var substitution in additional.Where(item => item.GetMetadata("EngineOption") == "substitutions"))
            {
                var argument = " --substitutions \"" + substitution.ItemSpec + "\"";
                var offset = engineArguments.IndexOf(argument, StringComparison.Ordinal);
                if (offset < 0 || offset != engineArguments.LastIndexOf(argument, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The SDK substitution file cannot be bound uniquely to its declared input.");
                }
                engineArguments = engineArguments.Remove(offset, argument.Length);
            }
            foreach (var step in additional.Where(item => item.GetMetadata("Kind") == "custom-step" &&
                         item.GetMetadata("Type") == "AtomUI.TypeMap.Linker.MaterializeTypeMapsStep"))
            {
                var bundled = Path.Combine(hostDirectory, "AtomUI.TypeMap.Linker.dll");
                if (Hash(step.ItemSpec) != Hash(bundled))
                {
                    throw new InvalidDataException("The browser materializer differs from the resolved bridge toolset.");
                }
                step.ItemSpec = bundled;
                ((TaskWireItem)step).FullPath = bundled;
                step.SetMetadata("AtomUIBundleRoot", hostDirectory);
            }
            foreach (var file in Directory.GetFiles(hostDirectory))
            {
                var item = new TaskWireItem { ItemSpec = file, FullPath = file };
                item.SetMetadata("Kind", "tool");
                item.SetMetadata("AtomUIBundleRoot", hostDirectory);
                additional.Add(item);
            }
            var runtimeList = Path.Combine(Path.GetFullPath(RuntimePackDirectory), "data", "RuntimeList.xml");
            var manifestItem = new TaskWireItem { ItemSpec = runtimeList, FullPath = runtimeList };
            manifestItem.SetMetadata("Kind", "configuration");
            additional.Add(manifestItem);
            var prepare = new PrepareConditionalRegistrationInputsTask
            {
                BuildEngine = BuildEngine,
                OutputRoot = Path.Combine(Path.GetFullPath(OutputRoot), "inputs"),
                Configuration = Configuration + "|bridge-v1|" + RuntimePackDirectory + "|" + ApplicationAssemblyIdentity,
                Assemblies = Assemblies,
                AdditionalInputs = additional.ToArray(),
                EngineArguments = engineArguments
            };
            if (!prepare.Execute())
            {
                return false;
            }
            InputsPath = prepare.InputsPath;
            foreach (var executable in prepare.PreparedAdditionalInputs.Where(item => item.GetMetadata("AtomUIExecutable") == "true"))
            {
                if (executable.GetMetadata("Kind") != "tool")
                {
                    throw new InvalidDataException("Only frozen tool inputs may request executable permissions: " +
                        executable.ItemSpec + " (" + executable.GetMetadata("Kind") + ").");
                }
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(executable.ItemSpec, UnixFileMode.UserRead | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                }
            }
            var host = FrozenTool(prepare.PreparedAdditionalInputs, Path.Combine(hostDirectory, "AtomUI.TypeMap.Linker.dll"));
            var cecil = FrozenTool(prepare.PreparedAdditionalInputs, Path.Combine(hostDirectory, "Mono.Cecil.dll"));
            var output = Path.Combine(Path.GetDirectoryName(InputsPath)!, "bridge");
            var start = new ProcessStartInfo(DotNetExecutable)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (var argument in new[] { host, "bridge", "--input", InputsPath, "--runtime-pack", Path.GetFullPath(RuntimePackDirectory),
                         "--application", ApplicationAssemblyIdentity, "--output", output, "--cecil", cecil })
            {
                start.ArgumentList.Add(argument);
            }
            using var process = Process.Start(start) ?? throw new InvalidDataException("The bridge tool host could not start.");
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                process.WaitForExitAsync(timeout.Token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                throw new InvalidDataException("The controlled bridge process exceeded its execution limit.");
            }
            Task.WhenAll(stdout, stderr).GetAwaiter().GetResult();
            if (process.ExitCode != 0)
            {
                throw new InvalidDataException("Conditional bridge failed: " + stderr.Result.Trim());
            }
            TransformationPath = Path.Combine(output, "transformation.json");
            using var transformation = JsonDocument.Parse(File.ReadAllText(TransformationPath));
            var root = transformation.RootElement;
            RegistrationContractJson.ValidateUniqueFields(root);
            if (root.GetProperty("format").GetInt32() != 1 || root.GetProperty("inputFormat").GetInt32() != 2 ||
                root.GetProperty("originalInputHash").GetString() != Hash(InputsPath) ||
                root.GetProperty("applicationIdentity").GetString() != ApplicationAssemblyIdentity)
            {
                throw new InvalidDataException("The bridge output does not belong to this immutable invocation.");
            }
            NeedsBridge = root.GetProperty("stage").GetString() == "transformed-input";
            if (!NeedsBridge && root.GetProperty("stage").GetString() != "unchanged-input")
            {
                throw new InvalidDataException("Unknown bridge transformation state.");
            }
            var replacements = root.GetProperty("transformed").EnumerateArray().ToDictionary(
                value => value.GetProperty("AssemblyIdentity").GetString()!, value => value, StringComparer.Ordinal);
            var prepared = new List<ITaskItem>();
            foreach (var input in prepare.PreparedAssemblies)
            {
                var item = TaskWireItem.FromItem(input);
                var identity = AssemblyName.GetAssemblyName(item.ItemSpec).FullName!;
                if (replacements.TryGetValue(identity, out var replacement))
                {
                    var target = Path.GetFullPath(replacement.GetProperty("outputPath").GetString()!);
                    if (!target.StartsWith(output + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
                        Hash(item.ItemSpec) != replacement.GetProperty("originalHash").GetString() ||
                        Hash(target) != replacement.GetProperty("transformedHash").GetString() ||
                        AssemblyName.GetAssemblyName(target).FullName != identity)
                    {
                        throw new InvalidDataException("Bridge replacement identity/content mismatch: " + identity);
                    }
                    item.SetMetadata("AtomUIFrozenPath", item.ItemSpec);
                    item.ItemSpec = target;
                    item.FullPath = target;
                }
                prepared.Add(item);
            }
            // A validated pure-net10 invocation keeps its original SDK item identities and behavior.
            PreparedAssemblies = NeedsBridge ? prepared.ToArray() : Assemblies;
            PreparedAdditionalInputs = NeedsBridge ? prepare.PreparedAdditionalInputs : AdditionalInputs;
            PreparedEngineArguments = NeedsBridge ? prepare.PreparedEngineArguments + string.Concat(
                prepare.PreparedAdditionalInputs.Where(item => item.GetMetadata("EngineOption") == "substitutions")
                    .Select(item => " --substitutions \"" + item.ItemSpec + "\"")) : EngineArguments;
            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or ArgumentException or
                                     BadImageFormatException or InvalidOperationException or JsonException or KeyNotFoundException)
        {
            NeedsBridge = false;
            PreparedAssemblies = [];
            PreparedAdditionalInputs = [];
            PreparedEngineArguments = string.Empty;
            TransformationPath = string.Empty;
            return Fail("ATOMUIREG006", error);
        }
    }

    private string PrepareHost()
    {
        var host = Path.GetFullPath(BridgeAssembly);
        var cecil = Path.GetFullPath(CecilAssembly);
        if (AssemblyName.GetAssemblyName(host).Name != "AtomUI.TypeMap.Linker" || AssemblyName.GetAssemblyName(cecil).Name != "Mono.Cecil")
        {
            throw new InvalidDataException("Unexpected bridge host or Cecil tool identity.");
        }
        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            ["AtomUI.TypeMap.Linker.dll"] = File.ReadAllBytes(host),
            ["AtomUI.TypeMap.Linker.deps.json"] = File.ReadAllBytes(Path.ChangeExtension(host, ".deps.json")),
            ["AtomUI.TypeMap.Linker.runtimeconfig.json"] = File.ReadAllBytes(Path.ChangeExtension(host, ".runtimeconfig.json")),
            ["Mono.Cecil.dll"] = File.ReadAllBytes(cecil)
        };
        var state = string.Join("\n", files.OrderBy(file => file.Key, StringComparer.Ordinal).Select(file =>
            file.Key + ":" + Hash(file.Value)));
        var fingerprint = Hash(Encoding.UTF8.GetBytes(state));
        var destination = Path.Combine(Path.GetFullPath(OutputRoot), "host", fingerprint);
        ToolBundleCache.Prepare(destination, files, "The frozen bridge tool bundle is incomplete or changed.");
        return destination;
    }

    private static string FrozenTool(IEnumerable<ITaskItem> items, string original) => items.Single(item =>
        item.GetMetadata("Kind") == "tool" && item.GetMetadata("AtomUIOriginalPath") == original).ItemSpec;

}
