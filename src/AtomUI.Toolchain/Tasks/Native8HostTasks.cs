using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Build.Framework;
using AtomUI.Build.Tasks.Registration;

namespace AtomUI.Build.Tasks;

/// <summary>Combines a maintainer-built managed ILC8 bundle with SDK-restored host native helpers.</summary>
public sealed class PrepareNative8RegistrationTask : RegistrationBuildTask
{
    private const string SourceCommit = "da7a9beabf4cccc624399d583f52f080a4c858c2";
    private static readonly string[] ManagedFiles =
    [
        "ilc.dll", "ilc.deps.json", "ilc.runtimeconfig.json", "System.CommandLine.dll",
        "ILCompiler.Compiler.dll", "ILCompiler.TypeSystem.dll", "ILCompiler.MetadataTransform.dll",
        "ILCompiler.DependencyAnalysisFramework.dll", "ILCompiler.RyuJit.dll"
    ];
    [Required] public string ManagedCompilerBundle { get; set; } = string.Empty;
    [Required] public string IlcHostPackagePath { get; set; } = string.Empty;
    [Required] public string HostRid { get; set; } = string.Empty;
    [Required] public string TargetRid { get; set; } = string.Empty;
    [Required] public string InvocationId { get; set; } = string.Empty;
    [Required] public string OwnedRoot { get; set; } = string.Empty;
    [Required] public string IntermediateRoot { get; set; } = string.Empty;
    [Required] public string DotNetHostPath { get; set; } = string.Empty;
    [Required] public string NativeObject { get; set; } = string.Empty;
    [Required] public string ExportsFile { get; set; } = string.Empty;
    [Required] public string NativeBinary { get; set; } = string.Empty;
    [Required] public string NativeLinker { get; set; } = string.Empty;
    [Output] public string ScopedIlcToolsPath { get; set; } = string.Empty;
    [Output] public string ScopedNativeLinkerPath { get; set; } = string.Empty;
    [Output] public string PrepareInputsRoot { get; set; } = string.Empty;
    [Output] public string ReceiptPointer { get; set; } = string.Empty;
    [Output] public string HostFingerprint { get; set; } = string.Empty;

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
            var outputs = new[] { NativeObject, ExportsFile, NativeBinary };
            var reserved = new[] { "invocation.json", "analysis.json", "inputs.json", "input-state.json", "receipt.json", "linked.json", "verified.json" }.Select(invocation.File);
            if (outputs.Distinct(Native8Invocation.PathComparer).Count() != outputs.Length || outputs.Intersect(reserved, Native8Invocation.PathComparer).Any())
            {
                throw new InvalidDataException("Native outputs must not alias each other or invocation protocol files.");
            }
            string invocationFile = invocation.File("invocation.json");
            using (var stream = new FileStream(invocationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                created.Add(invocationFile);
                JsonSerializer.Serialize(stream, new { format = 2, invocationId = InvocationId, ownedRoot = invocation.Root });
            }
            bool windows = OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture == Architecture.X64 && HostRid == "win-x64" && TargetRid == "win-x64";
            bool mac = OperatingSystem.IsMacOS() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64 && HostRid == "osx-arm64" && TargetRid is "osx-arm64" or "osx-x64";
            if (!windows && !mac)
            {
                throw new InvalidDataException("Native8 requires Windows x64 targeting win-x64, or macOS arm64 targeting osx-arm64/osx-x64.");
            }
            // SDK Exec invokes cmd.exe before our launcher, so literal percent expansion
            // cannot be escaped reliably by the managed driver at the later boundary.
            if (windows && new[] { invocation.Root, IntermediateRoot, DotNetHostPath, NativeLinker }.Any(path => path.Contains('%')))
            {
                throw new InvalidDataException("Windows SDK Exec does not support percent signs in Native8 launcher/output paths.");
            }
            var bundle = Path.GetFullPath(ManagedCompilerBundle);
            var nativePackage = Path.TrimEndingDirectorySeparator(Path.GetFullPath(IlcHostPackagePath));
            if (Path.GetFileName(nativePackage) != "8.0.33" ||
                !StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileName(Path.GetDirectoryName(nativePackage)), "runtime." + HostRid + ".microsoft.dotnet.ilcompiler"))
            {
                throw new InvalidDataException("Native8 requires the matching SDK-restored host compiler package at 8.0.33.");
            }
            var expectedNames = ManagedFiles.Append("atomui-ilc8-capability.json").ToHashSet(StringComparer.Ordinal);
            if (!Directory.GetFiles(bundle).Select(Path.GetFileName).ToHashSet(StringComparer.Ordinal).SetEquals(expectedNames) ||
                Directory.GetDirectories(bundle).Length != 0)
            {
                throw new InvalidDataException("Managed ILC8 bundle must contain exactly the audited file whitelist.");
            }
            using var capability = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(bundle, "atomui-ilc8-capability.json")));
            var info = capability.RootElement;
            if (info.GetProperty("format").GetInt32() != 1 || info.GetProperty("sourceCommit").GetString() != SourceCommit ||
                info.GetProperty("nativeToolPackageVersion").GetString() != "8.0.33" ||
                info.GetProperty("hostTargetFramework").GetString() != "net10.0" || info.GetProperty("inputFormat").GetInt32() != 2 ||
                info.GetProperty("collectorTemplate").GetString() != "atomui.collector.v1" ||
                info.GetProperty("trimmedSwitchTemplate").GetString() != "atomui.trimmed-switch.v1" ||
                !info.GetProperty("files").EnumerateObject().Select(entry => entry.Name).ToHashSet(StringComparer.Ordinal).SetEquals(ManagedFiles))
            {
                throw new InvalidDataException("Unknown native compiler capability or upstream version.");
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
            foreach (string name in windows ? new[] { "jitinterface_x64.dll", "objwriter.dll", "clrjit_win_x64_x64.dll" } : new[] { "libjitinterface_arm64.dylib", "libobjwriter.dylib",
                         TargetRid == "osx-arm64" ? "libclrjit_universal_arm64_arm64.dylib" : "libclrjit_unix_x64_arm64.dylib" })
            {
                files.Add("host/" + name, File.ReadAllBytes(Path.Combine(nativePackage, "tools", name)));
            }
            var root = Path.GetFullPath(IntermediateRoot);
            PrepareInputsRoot = Path.Combine(root, "inputs");
            ReceiptPointer = invocation.File("receipt.json");
            string baseLine = "#!/bin/sh\nbase=$(CDPATH= cd -- \"$(dirname -- \"$0\")/..\" && pwd)\n";
            string cleanup = Quote(Path.GetFullPath(DotNetHostPath)) + " \"$base/host/ilc.dll\" --atomui-cleanup-invocation " +
                string.Join(" ", new[] { invocation.Root, InvocationId, NativeObject, ExportsFile, NativeBinary }.Select(Quote));
            string failed = "\nstatus=$?\nif [ \"$status\" -ne 0 ]; then\n  " + cleanup + "\nfi\nexit \"$status\"\n";
            string launch = baseLine + Quote(Path.GetFullPath(DotNetHostPath)) + " \"$base/host/ilc.dll\" \"$@\"" + failed;
            if (windows)
            {
                string hostPath = Path.GetFullPath(DotNetHostPath);
                string? inheritedHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
                if (string.IsNullOrEmpty(inheritedHost) || !Path.IsPathFullyQualified(inheritedHost) ||
                    !Native8Invocation.PathComparer.Equals(Path.GetFullPath(inheritedHost), hostPath) || Hash(inheritedHost) != Hash(hostPath))
                {
                    throw new InvalidDataException("Windows Native8 requires inherited DOTNET_HOST_PATH to match the captured dotnet host.");
                }
                // Keep the batch source ASCII: cmd decodes source using the caller's console
                // code page. Unicode fixed paths travel only in the fingerprinted payload;
                // the host and launcher directory come from Unicode environment expansion.
                string configuration = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(new[]
                {
                    hostPath, Hash(hostPath), invocation.Root, InvocationId, NativeObject, ExportsFile, NativeBinary, NativeLinker
                }));
                string prefix = "@echo off\r\nsetlocal DisableDelayedExpansion\r\nif not defined DOTNET_HOST_PATH exit /b 87\r\n";
                string driver = "\"%DOTNET_HOST_PATH%\" \"%~dp0..\\host\\ilc.dll\"";
                files.Add("tools/ilc.cmd", Encoding.ASCII.GetBytes(prefix + driver + " --atomui-compiler-config " + configuration + " -- %*\r\nexit /b %errorlevel%\r\n"));
                files.Add("tools/native-linker.cmd", Encoding.ASCII.GetBytes(prefix + driver + " --atomui-native-link-config " + configuration + " -- %*\r\nexit /b %errorlevel%\r\n"));
            }
            else
            {
                files.Add("tools/ilc", Encoding.UTF8.GetBytes(launch));
                files.Add("tools/native-linker", Encoding.UTF8.GetBytes(baseLine + Quote(NativeLinker) + " \"$@\"" + failed));
            }
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
            ScopedNativeLinkerPath = Path.Combine(destination, "tools", windows ? "native-linker.cmd" : "native-linker");
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or InvalidDataException or JsonException or InvalidOperationException or KeyNotFoundException)
        {
            invocation?.DeleteOwned(created);
            ScopedIlcToolsPath = ScopedNativeLinkerPath = PrepareInputsRoot = ReceiptPointer = HostFingerprint = string.Empty;
            return Fail("ATOMUIREG008", error);
        }
    }

    private static string Quote(string value) => "'" + value.Replace("'", "'\\''", StringComparison.Ordinal) + "'";
}
