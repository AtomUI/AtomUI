using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Build.Framework;
using AtomUI.Build.Tasks.Registration;

namespace AtomUI.Build.Tasks;

/// <summary>
/// Combines a maintainer-built managed ILC8 bundle with SDK-restored host native helpers.
/// </summary>
public sealed class PrepareNative8RegistrationTask : RegistrationBuildTask
{
    private const string SourceCommit = "a6bde67c455f2ac219988c7a66171631090b6f65";
    private static readonly string[] ManagedFiles =
    [
        "ilc.dll", "ilc.deps.json", "ilc.runtimeconfig.json", "System.CommandLine.dll",
        "ILCompiler.Compiler.dll", "ILCompiler.TypeSystem.dll", "ILCompiler.MetadataTransform.dll",
        "ILCompiler.DependencyAnalysisFramework.dll", "ILCompiler.RyuJit.dll"
    ];
    /// <summary>
    /// Directory of the maintainer-built managed ILC8 compiler. It must contain exactly the audited compiler files
    /// and the <c>atomui-ilc8-capability.json</c> that describes them, with no subdirectory and no symbolic link,
    /// and every compiler file must match the hash that the capability file records.
    /// </summary>
    [Required] public string ManagedCompilerBundle { get; set; } = string.Empty;
    /// <summary>
    /// Directory of the SDK-restored <c>runtime.osx-arm64.microsoft.dotnet.ilcompiler</c> 8.x package. The native
    /// helpers <c>libjitinterface</c>, <c>libobjwriter</c> and the JIT for <see cref="TargetRid" /> are read from
    /// its <c>tools</c> subdirectory.
    /// </summary>
    [Required] public string IlcHostPackagePath { get; set; } = string.Empty;
    /// <summary>
    /// Runtime identifier of the build host. It must be <c>osx-arm64</c>, and the process must run on macOS ARM64;
    /// any other host or target fails the task. It is part of <see cref="HostFingerprint" />.
    /// </summary>
    [Required] public string HostRid { get; set; } = string.Empty;
    /// <summary>
    /// Runtime identifier that the native compilation targets, <c>osx-arm64</c> or <c>osx-x64</c>. It selects the
    /// JIT helper that is copied from <see cref="IlcHostPackagePath" /> and is part of
    /// <see cref="HostFingerprint" />.
    /// </summary>
    [Required] public string TargetRid { get; set; } = string.Empty;
    /// <summary>
    /// Identifier of this native compilation invocation. The task records it with <see cref="OwnedRoot" /> in the
    /// invocation file under that root, and the launchers pass it to the cleanup command that they run after a
    /// failure.
    /// </summary>
    [Required] public string InvocationId { get; set; } = string.Empty;
    /// <summary>
    /// Absolute root directory of this invocation. <see cref="NativeObject" />, <see cref="ExportsFile" /> and
    /// <see cref="NativeBinary" /> must lie under it; the task replaces them with the result of that ownership
    /// check. The invocation file is written under this root, the receipt is expected under it, and a failed run
    /// deletes the files that it created there.
    /// </summary>
    [Required] public string OwnedRoot { get; set; } = string.Empty;
    /// <summary>
    /// Root directory for what this task derives from its inputs: the inputs directory
    /// <see cref="PrepareInputsRoot" />, the prepared host under <c>hosts</c> and the lock file that serializes the
    /// preparation of a host.
    /// </summary>
    [Required] public string IntermediateRoot { get; set; } = string.Empty;
    /// <summary>
    /// Path of the dotnet host that executes the managed compiler. The generated <c>ilc</c> launcher runs
    /// <c>host/ilc.dll</c> through it with <c>exec --roll-forward Major</c>, and the cleanup command appended to
    /// both launchers runs the compiler through it with <c>--atomui-cleanup-invocation</c>.
    /// </summary>
    [Required] public string DotNetHostPath { get; set; } = string.Empty;
    /// <summary>
    /// Path of the object file that the native compilation produces. It must lie under <see cref="OwnedRoot" />
    /// and is passed to the cleanup command that the launchers run after a failure.
    /// </summary>
    [Required] public string NativeObject { get; set; } = string.Empty;
    /// <summary>
    /// Path of the export list that the native compilation produces, one symbol per line. It must lie under
    /// <see cref="OwnedRoot" /> and is passed to the cleanup command that the launchers run after a failure.
    /// </summary>
    [Required] public string ExportsFile { get; set; } = string.Empty;
    /// <summary>
    /// Path of the native binary that the native compilation produces. It must lie under <see cref="OwnedRoot" />
    /// and is passed to the cleanup command that the launchers run after a failure.
    /// </summary>
    [Required] public string NativeBinary { get; set; } = string.Empty;
    /// <summary>
    /// Path of the native linker. The generated <c>native-linker</c> launcher calls it with every argument it
    /// received and, when the linker exits non-zero, runs the cleanup command of this invocation.
    /// </summary>
    [Required] public string NativeLinker { get; set; } = string.Empty;
    /// <summary>
    /// The <c>tools</c> directory of the prepared host, with a trailing directory separator. It holds the generated
    /// <c>ilc</c> and <c>native-linker</c> launchers. It is empty when the task fails.
    /// </summary>
    [Output] public string ScopedIlcToolsPath { get; set; } = string.Empty;
    /// <summary>
    /// Path of the generated <c>native-linker</c> launcher in the prepared host. It is empty when the task fails.
    /// </summary>
    [Output] public string ScopedNativeLinkerPath { get; set; } = string.Empty;
    /// <summary>
    /// Directory <c>inputs</c> under <see cref="IntermediateRoot" /> that this task designates for the inputs of
    /// the native compilation. The task only names the directory and does not create it. It is empty when the
    /// task fails.
    /// </summary>
    [Output] public string PrepareInputsRoot { get; set; } = string.Empty;
    /// <summary>
    /// Path of the <c>receipt.json</c> file of this invocation under <see cref="OwnedRoot" />, where the receipt of
    /// the native compilation is expected and is read back by the verification step. It is empty when the task
    /// fails.
    /// </summary>
    [Output] public string ReceiptPointer { get; set; } = string.Empty;
    /// <summary>
    /// SHA-256 of the serialized host state: the host and target RIDs with the path and hash of every file that is
    /// copied into the prepared host. It names the host directory under <c>hosts</c> and the lock file that guards
    /// its preparation. It is empty when the task fails.
    /// </summary>
    [Output] public string HostFingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Validates the invocation, the compiler bundle and the host package, then prepares the native host. It writes
    /// <c>invocation.json</c> under <see cref="OwnedRoot" />, assembles the compiler files, the native helpers and
    /// the generated launchers, and stores them in the directory under <c>hosts</c> in
    /// <see cref="IntermediateRoot" /> that <see cref="HostFingerprint" /> names, so that an unchanged host is
    /// prepared only once. Returns <see langword="true" /> when the host is ready. Otherwise it deletes the files
    /// that it created under <see cref="OwnedRoot" />, empties every output, logs error ATOMUIREG008 and returns
    /// <see langword="false" />.
    /// </summary>
    public override bool Execute()
    {
        Native8Invocation? invocation = null;
        var created = new List<string>();
        try
        {
            invocation = new Native8Invocation(InvocationId, OwnedRoot);
            NativeObject = invocation.RequireOwned(Path.GetFullPath(NativeObject));
            ExportsFile = invocation.RequireOwned(Path.GetFullPath(ExportsFile));
            NativeBinary = invocation.RequireOwned(Path.GetFullPath(NativeBinary));
            string invocationFile = invocation.File("invocation.json");
            using (var stream = new FileStream(invocationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                created.Add(invocationFile);
                JsonSerializer.Serialize(stream, new { format = 2, invocationId = InvocationId, ownedRoot = invocation.Root });
            }
            if (!OperatingSystem.IsMacOS() || RuntimeInformation.ProcessArchitecture != Architecture.Arm64 ||
                HostRid != "osx-arm64" || TargetRid is not ("osx-arm64" or "osx-x64"))
            {
                throw new InvalidDataException("Native8 opt-in is verified only on osx-arm64 hosts targeting osx-arm64 or osx-x64.");
            }
            var bundle = Path.GetFullPath(ManagedCompilerBundle);
            var nativePackage = Path.TrimEndingDirectorySeparator(Path.GetFullPath(IlcHostPackagePath));
            var expectedNames = ManagedFiles.Append("atomui-ilc8-capability.json").ToHashSet(StringComparer.Ordinal);
            if (!Directory.GetFiles(bundle).Select(Path.GetFileName).ToHashSet(StringComparer.Ordinal).SetEquals(expectedNames) ||
                Directory.GetDirectories(bundle).Length != 0)
            {
                throw new InvalidDataException("Managed ILC8 bundle must contain exactly the audited file whitelist.");
            }
            using var capability = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(bundle, "atomui-ilc8-capability.json")));
            var info = capability.RootElement;
            if (info.GetProperty("format").GetInt32() != 1 || info.GetProperty("sourceCommit").GetString() != SourceCommit ||
                !Net8EngineToolchain.IsEngineVersion(info.GetProperty("nativeToolPackageVersion").GetString() ?? string.Empty) ||
                info.GetProperty("hostTargetFramework").GetString() != "net10.0" || info.GetProperty("inputFormat").GetInt32() != 2 ||
                info.GetProperty("collectorTemplate").GetString() != "atomui.collector.v1" ||
                info.GetProperty("trimmedSwitchTemplate").GetString() != "atomui.trimmed-switch.v1" ||
                !info.GetProperty("files").EnumerateObject().Select(entry => entry.Name).ToHashSet(StringComparer.Ordinal).SetEquals(ManagedFiles))
            {
                throw new InvalidDataException("Unknown native compiler capability or upstream version.");
            }
            if (!Net8EngineToolchain.IsSupportedNativeCompilerPackage(nativePackage))
            {
                throw new InvalidDataException($"Native8 requires SDK-restored runtime.osx-arm64.microsoft.dotnet.ilcompiler 8.x; found {nativePackage}.");
            }
            var files = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);
            foreach (string name in expectedNames)
            {
                var path = Path.Combine(bundle, name);
                if (new FileInfo(path).LinkTarget is not null)
                {
                    throw new InvalidDataException("Managed compiler files must not be symbolic links.");
                }
                var bytes = File.ReadAllBytes(path);
                if (name != "atomui-ilc8-capability.json" && Hash(bytes) != info.GetProperty("files").GetProperty(name).GetString())
                {
                    throw new InvalidDataException("Managed compiler bundle hash mismatch: " + name);
                }
                files.Add("host/" + name, bytes);
            }
            foreach (string name in new[] { "libjitinterface_arm64.dylib", "libobjwriter.dylib",
                         TargetRid == "osx-arm64" ? "libclrjit_universal_arm64_arm64.dylib" : "libclrjit_unix_x64_arm64.dylib" })
            {
                files.Add("host/" + name, File.ReadAllBytes(Path.Combine(nativePackage, "tools", name)));
            }
            var root = Path.GetFullPath(IntermediateRoot);
            PrepareInputsRoot = Path.Combine(root, "inputs");
            ReceiptPointer = invocation.File("receipt.json");
            string[] cleanupArguments = [invocation.Root, InvocationId, NativeObject, ExportsFile, NativeBinary];
            string failed = FailureCleanup(Path.GetFullPath(DotNetHostPath), cleanupArguments);
            files.Add("tools/ilc", Encoding.UTF8.GetBytes(CreateCompilerLauncher(Path.GetFullPath(DotNetHostPath), cleanupArguments)));
            files.Add("tools/native-linker", Encoding.UTF8.GetBytes(LauncherPrologue + Quote(NativeLinker) + " \"$@\"" + failed));
            var state = JsonSerializer.Serialize(new { format = 1, HostRid, TargetRid,
                files = files.Select(file => new { path = file.Key, sha256 = Hash(file.Value) }).ToArray() });
            HostFingerprint = Hash(Encoding.UTF8.GetBytes(state));
            var destination = Path.Combine(root, "hosts", HostFingerprint);
            using var ownership = ToolBundleCache.Acquire(Path.Combine(root, HostFingerprint + ".lock"), TimeSpan.FromSeconds(60), 50);
            files.Add("host-state.json", Encoding.UTF8.GetBytes(state));
            ToolBundleCache.Prepare(destination, files, "Prepared native host was changed; refusing cache reuse.",
                static (relative, path) =>
                {
                    if (relative.StartsWith("tools/", StringComparison.Ordinal) && !OperatingSystem.IsWindows())
                    {
                        File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                            UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
                    }
                });
            ScopedIlcToolsPath = Path.Combine(destination, "tools") + Path.DirectorySeparatorChar;
            ScopedNativeLinkerPath = Path.Combine(destination, "tools", "native-linker");
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or InvalidDataException or JsonException or InvalidOperationException or KeyNotFoundException)
        {
            invocation?.DeleteOwned(created);
            ScopedIlcToolsPath = ScopedNativeLinkerPath = PrepareInputsRoot = ReceiptPointer = HostFingerprint = string.Empty;
            return Fail("ATOMUIREG008", error);
        }
    }

    private const string LauncherPrologue = "#!/bin/sh\nbase=$(CDPATH= cd -- \"$(dirname -- \"$0\")/..\" && pwd)\n";

    // The managed compiler targets net10.0; Major roll-forward lets hosts with only a newer runtime execute it.
    private static string CompilerCommand(string dotNetHostPath) =>
        Quote(dotNetHostPath) + " exec --roll-forward Major \"$base/host/ilc.dll\"";

    private static string FailureCleanup(string dotNetHostPath, IEnumerable<string> cleanupArguments) =>
        "\nstatus=$?\nif [ \"$status\" -ne 0 ]; then\n  " + CompilerCommand(dotNetHostPath) + " --atomui-cleanup-invocation " +
        string.Join(" ", cleanupArguments.Select(Quote)) + "\nfi\nexit \"$status\"\n";

    internal static string CreateCompilerLauncher(string dotNetHostPath, IReadOnlyList<string> cleanupArguments) =>
        LauncherPrologue + CompilerCommand(dotNetHostPath) + " \"$@\"" + FailureCleanup(dotNetHostPath, cleanupArguments);

    private static string Quote(string value) => "'" + value.Replace("'", "'\\''", StringComparison.Ordinal) + "'";
}
