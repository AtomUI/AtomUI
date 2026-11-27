using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AtomUI.Build.Tasks.Isolation;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Prepares an isolated official-TypeMap input set without loading consumer assemblies.
/// It probes whether <see cref="Assemblies" /> need the conditional bridge and, when they do, freezes the
/// inputs, runs the bridge tool in a separate process and validates the transformation it wrote. Without a
/// bridge the inputs pass through to the outputs unchanged.
/// </summary>
public sealed class PrepareConditionalRegistrationBridgeTask : RegistrationBuildTask
{
    /// <summary>
    /// Root under which this task writes: the frozen input snapshot in its <c>inputs</c> directory and the copy of
    /// the bridge tool in its <c>host</c> directory.
    /// </summary>
    [Required] public string OutputRoot { get; set; } = string.Empty;
    /// <summary>
    /// Text that identifies the settings of the invocation, such as the target framework, the runtime identifier
    /// and the build configuration. The task appends <c>bridge-v1</c>, the <see cref="RuntimePackDirectory" /> and
    /// the <see cref="ApplicationAssemblyIdentity" /> to it; the input preparation records the result with the
    /// frozen snapshot.
    /// </summary>
    [Required] public string Configuration { get; set; } = string.Empty;
    /// <summary>
    /// The assemblies to prepare for the engine, each item spec a managed assembly path: the linker inputs of the
    /// managed route, or the compile inputs, references and satellite assemblies of the NativeAOT route. The probe
    /// that decides whether the bridge is needed looks for <c>AtomUI.Core</c> among them: none means no bridge, and
    /// more than one <c>AtomUI.Core</c> identity fails the task.
    /// </summary>
    public ITaskItem[] Assemblies { get; set; } = [];
    /// <summary>
    /// Inputs other than <see cref="Assemblies" />. An item whose <c>EngineOption</c> is <c>substitutions</c> must
    /// be quoted in <see cref="EngineArguments" /> exactly once, and a <c>custom-step</c> item of type
    /// <c>AtomUI.TypeMap.Linker.MaterializeTypeMapsStep</c> must have the same content as
    /// <see cref="BridgeAssembly" />; the step is then redirected to the bundled copy. Only a <c>tool</c> item may
    /// carry <c>AtomUIExecutable=true</c>, and its frozen copy gets executable permission outside Windows. The task
    /// adds the host bundle files as <c>tool</c> items and the <c>RuntimeList.xml</c> of
    /// <see cref="RuntimePackDirectory" /> as a <c>configuration</c> item.
    /// </summary>
    public ITaskItem[] AdditionalInputs { get; set; } = [];
    /// <summary>
    /// The engine arguments the SDK produced. Each substitution input declared in <see cref="AdditionalInputs" />
    /// must be quoted in them exactly once as a <c>--substitutions</c> argument, otherwise the task fails. Those
    /// arguments are removed before the input preparation sees them, and <see cref="PreparedEngineArguments" />
    /// names the frozen copies instead.
    /// </summary>
    public string EngineArguments { get; set; } = string.Empty;
    /// <summary>
    /// Path of the <c>AtomUI.TypeMap.Linker.dll</c> bridge tool. Its assembly name must be
    /// <c>AtomUI.TypeMap.Linker</c>, otherwise the task fails. It is copied with its <c>.deps.json</c> and
    /// <c>.runtimeconfig.json</c> into a content-fingerprinted host bundle under <see cref="OutputRoot" />, which
    /// is then frozen with the other inputs.
    /// </summary>
    [Required] public string BridgeAssembly { get; set; } = string.Empty;
    /// <summary>
    /// Path of the <c>Mono.Cecil.dll</c> the bridge tool uses. Its assembly name must be <c>Mono.Cecil</c>,
    /// otherwise the task fails. It is copied into the host bundle, and the frozen copy is passed to the bridge
    /// tool as <c>--cecil</c>.
    /// </summary>
    [Required] public string CecilAssembly { get; set; } = string.Empty;
    /// <summary>
    /// Directory of the runtime pack resolved for the target runtime. Its <c>data/RuntimeList.xml</c> is added to
    /// the inputs as a <c>configuration</c> item, and the directory is passed to the bridge tool as
    /// <c>--runtime-pack</c> and appended to <see cref="Configuration" />.
    /// </summary>
    [Required] public string RuntimePackDirectory { get; set; } = string.Empty;
    /// <summary>
    /// Full name of the application assembly the bridge runs for. It is passed to the bridge tool as
    /// <c>--application</c>, appended to <see cref="Configuration" /> and compared with the
    /// <c>applicationIdentity</c> the bridge reports; a mismatch fails the task.
    /// </summary>
    [Required] public string ApplicationAssemblyIdentity { get; set; } = string.Empty;
    /// <summary>
    /// Path of the .NET host executable that starts the bridge tool process. The tool is started from the frozen
    /// copy of the host bundle with the command <c>bridge</c>.
    /// </summary>
    [Required] public string DotNetExecutable { get; set; } = string.Empty;
    /// <summary>
    /// When <see langword="true" />, only the probe runs: <see cref="NeedsBridge" /> reports whether
    /// <see cref="Assemblies" /> need the bridge, <see cref="PreparedAssemblies" />,
    /// <see cref="PreparedAdditionalInputs" /> and <see cref="PreparedEngineArguments" /> pass the inputs through
    /// unchanged, nothing is written under <see cref="OutputRoot" /> and the bridge tool is not started.
    /// </summary>
    public bool ProbeOnly { get; set; }
    /// <summary>
    /// Whether the engine must run on the bridge result instead of the original inputs. It starts as the result of
    /// probing <see cref="Assemblies" />. When the bridge tool ran, the stage it reports decides the final value:
    /// <c>transformed-input</c> means <see langword="true" /> and <c>unchanged-input</c> means
    /// <see langword="false" />. When this task fails it is <see langword="false" />, unless the input preparation
    /// failed, which leaves the probe result in place.
    /// </summary>
    [Output] public bool NeedsBridge { get; set; }
    /// <summary>
    /// Path of the input list that names the frozen inputs, as written by the input preparation. The bridge tool
    /// reads it as <c>--input</c> and has to report its hash as <c>originalInputHash</c>. Unlike the other outputs
    /// it is not cleared when the task fails.
    /// </summary>
    [Output] public string InputsPath { get; set; } = string.Empty;
    /// <summary>
    /// Path of the <c>transformation.json</c> the bridge tool wrote into the <c>bridge</c> directory next to
    /// <see cref="InputsPath" />. It records the stage the bridge reached and each assembly it transformed. Empty
    /// when the bridge tool did not run or the task failed.
    /// </summary>
    [Output] public string TransformationPath { get; set; } = string.Empty;
    /// <summary>
    /// The assemblies to use instead of <see cref="Assemblies" />. They are unchanged when
    /// <see cref="NeedsBridge" /> is <see langword="false" />. Otherwise every item points at its frozen copy, or
    /// at the transformed assembly when the bridge replaced it; such an item records the frozen copy in the
    /// <c>AtomUIFrozenPath</c> metadata. Empty when the task fails.
    /// </summary>
    [Output] public ITaskItem[] PreparedAssemblies { get; set; } = [];
    /// <summary>
    /// The additional inputs to use instead of <see cref="AdditionalInputs" />. They are unchanged when
    /// <see cref="NeedsBridge" /> is <see langword="false" />; otherwise they are the frozen copies the input
    /// preparation produced, which also include the bridge host bundle files and the runtime pack manifest.
    /// Empty when the task fails.
    /// </summary>
    [Output] public ITaskItem[] PreparedAdditionalInputs { get; set; } = [];
    /// <summary>
    /// The engine arguments to use instead of <see cref="EngineArguments" />. They are unchanged when
    /// <see cref="NeedsBridge" /> is <see langword="false" />; otherwise they are the arguments the input
    /// preparation produced, followed by one quoted <c>--substitutions</c> argument per frozen substitution input.
    /// Empty when the task fails.
    /// </summary>
    [Output] public string PreparedEngineArguments { get; set; } = string.Empty;

    /// <summary>
    /// Probes <see cref="Assemblies" /> and stops there when <see cref="ProbeOnly" /> is set or no bridge is
    /// needed, passing the inputs through to the outputs. Otherwise it copies the bridge tool into a host bundle,
    /// has the input preparation freeze the inputs, runs the bridge tool on them in a separate process and checks
    /// the transformation it wrote before the prepared outputs are set.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the outputs are ready for the engine, also when the inputs only passed through.
    /// <see langword="false" /> when the probe rejects the assemblies, an input is inconsistent, the input
    /// preparation fails, the bridge tool fails or exceeds its five-minute limit, or its output does not belong to
    /// this invocation. The error is then logged as <c>ATOMUIREG006</c>.
    /// </returns>
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
