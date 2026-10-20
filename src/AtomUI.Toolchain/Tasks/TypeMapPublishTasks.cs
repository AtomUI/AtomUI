using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

internal static class TypeMapBuildContract
{
    internal const string Capability = "atomui-typemap-v1-illink-10.0.8";
    internal const string LinkerIdentity = "illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
    internal const string LinkerVersion = "10.0.8-servicing.26229.119+94ea82652cdd4e0f8046b5bd5becbd11461482ca";
    // Desktop trimming consumes the official TypeMap contract with a toolchain matching the target framework.
    // The verified servicing floor still applies; the Browser backend keeps its separate exact pins.
    internal static readonly Version DesktopToolchainFloor = new(10, 0, 8);
    private const string MicrosoftPublicKeyToken = "31BF3856AD364E35";

    internal static bool IsSupportedDesktopToolchain(string informationalVersion, Version targetFrameworkVersion)
    {
        var end = informationalVersion.IndexOfAny(['-', '+']);
        var numeric = end < 0 ? informationalVersion : informationalVersion[..end];
        return Version.TryParse(numeric.Trim(), out var version) &&
               version.Major == targetFrameworkVersion.Major &&
               version >= new Version(targetFrameworkVersion.Major, targetFrameworkVersion.Minor) &&
               version >= DesktopToolchainFloor;
    }

    internal static bool IsSupportedDesktopLinker(string identity, string informationalVersion, Version targetFrameworkVersion)
    {
        var name = new AssemblyName(identity);
        return name.Name == "illink" &&
               name.Version?.Major == targetFrameworkVersion.Major &&
               Convert.ToHexString(name.GetPublicKeyToken() ?? []) == MicrosoftPublicKeyToken &&
               IsSupportedDesktopToolchain(informationalVersion, targetFrameworkVersion);
    }
    internal static string Hash(string path) => ResolveRegistrationToolsTask.Hash(path);
    internal static string InformationalVersion(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var reader = pe.GetMetadataReader();
        foreach (var handle in reader.GetAssemblyDefinition().GetCustomAttributes())
        {
            var attribute = reader.GetCustomAttribute(handle);
            if (attribute.Constructor.Kind != HandleKind.MemberReference)
            {
                continue;
            }

            var member = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
            if (member.Parent.Kind != HandleKind.TypeReference)
            {
                continue;
            }

            var type = reader.GetTypeReference((TypeReferenceHandle)member.Parent);
            if (reader.GetString(type.Namespace) != "System.Reflection" || reader.GetString(type.Name) != "AssemblyInformationalVersionAttribute")
            {
                continue;
            }

            var value = reader.GetBlobReader(attribute.Value);
            if (value.ReadUInt16() != 1)
            {
                break;
            }

            return value.ReadSerializedString() ?? string.Empty;
        }
        throw new InvalidDataException($"Missing informational identity: '{path}'.");
    }
}

public abstract class RegistrationBuildTask : ITask
{
    public IBuildEngine BuildEngine { get; set; } = null!;
    public ITaskHost HostObject { get; set; } = null!;
    public abstract bool Execute();
    protected bool Fail(string code, Exception error)
    {
        BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Registration", code, BuildEngine.ProjectFileOfTaskNode, 0, 0, 0, 0,
            error.Message, null, GetType().Name));
        return false;
    }
}

internal sealed record TypeMapFile(string Path, string Hash);
internal sealed record TypeMapLinkInput(string Path, string Identity, string Hash, string TrimMode);
internal sealed record TypeMapLinkSignature(string Capability, string BackendPath, string BackendIdentity, string BackendHash,
    string LinkerPath, string LinkerIdentity, string LinkerVersion, string ReceiptPath, string OutputDirectory,
    string Configuration, TypeMapFile[] Files, TypeMapLinkInput[] Assemblies, TypeMapLinkConfiguration? EffectiveLink = null);

public sealed class PrepareTypeMapLinkTask : RegistrationBuildTask
{
    public string SdkVersion { get; set; } = string.Empty;
    public string WasmSdkTasksPath { get; set; } = string.Empty;
    public string BackendAssembly { get; set; } = string.Empty;
    public string LinkerAssembly { get; set; } = string.Empty;
    public string Capability { get; set; } = TypeMapBuildContract.Capability;
    public string ReceiptPath { get; set; } = string.Empty;
    public string SignaturePath { get; set; } = string.Empty;
    public string LinkSemaphore { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public string Configuration { get; set; } = string.Empty;
    public ITaskItem[] Inputs { get; set; } = [];
    public ITaskItem[] Assemblies { get; set; } = [];
    public ITaskItem[] LinkOptions { get; set; } = [];
    public ITaskItem[] LinkItems { get; set; } = [];

    public override bool Execute()
    {
        try
        {
            if (SdkVersion != "10.0.300")
            {
                throw new InvalidDataException($"Unsupported SDK '{SdkVersion}'; AtomUI TypeMap publish requires SDK 10.0.300.");
            }

            if (Capability != TypeMapBuildContract.Capability)
            {
                throw new InvalidDataException($"Unsupported backend capability '{Capability}'.");
            }

            if (!Path.IsPathFullyQualified(ReceiptPath))
            {
                throw new InvalidDataException("AtomUITypeMapReceipt must be an absolute path.");
            }

            if (AssemblyName.GetAssemblyName(LinkerAssembly).FullName != TypeMapBuildContract.LinkerIdentity ||
                TypeMapBuildContract.InformationalVersion(LinkerAssembly) != TypeMapBuildContract.LinkerVersion)
            {
                throw new InvalidDataException($"Unsupported actual ILLink binary '{LinkerAssembly}'. Expected ILLink 10.0.8.");
            }
            // Resolve the SDK tasks to its installed pack. The running task assembly's metadata, not a TFM guess,
            // supplies the actual workload patch identity (the task version starts with the pack version).
            if (!File.Exists(WasmSdkTasksPath) || !TypeMapBuildContract.InformationalVersion(WasmSdkTasksPath).StartsWith("10.0.10", StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Unsupported actual WASM SDK tasks '{WasmSdkTasksPath}'. Expected 10.0.10.");
            }

            var toolDirectory = Path.GetDirectoryName(BackendAssembly)!;
            var linkerDirectory = Path.GetDirectoryName(LinkerAssembly)!;
            var assemblies = Assemblies.Select(i => new TypeMapLinkInput(Path.GetFullPath(i.ItemSpec),
                    AssemblyName.GetAssemblyName(i.ItemSpec).FullName!, TypeMapBuildContract.Hash(i.ItemSpec),
                    i.GetMetadata("TrimMode") + ":" + i.GetMetadata("IsTrimmable")))
                .OrderBy(a => a.Identity, StringComparer.Ordinal).ToArray();
            // ILLink resolves explicitly supplied implementation assemblies before reference-only candidates.
            // Avalonia temporarily adds net8 design-time references during the outer WASM build; the nested
            // publish omits them. Equal full identities already supplied in AssemblyPaths are shadowed inputs.
            var referencePaths = RegistrationReferenceInputs.Select(Inputs.Where(i => i.GetMetadata("Kind") == "Reference"), assemblies.Select(a => a.Identity)).ToHashSet(StringComparer.Ordinal);
            var effectiveLink = TypeMapLinkConfiguration.Capture(LinkOptions, LinkItems, Assemblies,
                Inputs.Where(i => i.GetMetadata("Kind") == "Reference" && referencePaths.Contains(i.ItemSpec)).ToArray());
            var files = RegistrationReferenceInputs.Select(Inputs, assemblies.Select(a => a.Identity))
                .Concat(effectiveLink.InputFiles(Inputs.Where(i => i.GetMetadata("Kind") == "Tool").Select(i => i.ItemSpec).Append(LinkerAssembly)))
                .Concat(ResolveRegistrationToolsTask.BackendFiles.Select(n => Path.Combine(toolDirectory, n)))
                .Concat(ResolveRegistrationToolsTask.TaskFiles.Select(n => Path.Combine(Path.GetDirectoryName(typeof(PrepareTypeMapLinkTask).Assembly.Location)!, n)))
                .Concat(new[] { LinkerAssembly, Path.Combine(linkerDirectory, "Mono.Cecil.dll"), Path.Combine(linkerDirectory, "illink.deps.json"),
                    Path.Combine(linkerDirectory, "illink.runtimeconfig.json"), WasmSdkTasksPath, typeof(PrepareTypeMapLinkTask).Assembly.Location })
                .Where(p => !string.IsNullOrWhiteSpace(p)).Select(Path.GetFullPath).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
                .Select(p => new TypeMapFile(p, TypeMapBuildContract.Hash(p))).ToArray();
            var signature = new TypeMapLinkSignature(Capability, Path.GetFullPath(BackendAssembly), AssemblyName.GetAssemblyName(BackendAssembly).FullName!,
                TypeMapBuildContract.Hash(BackendAssembly), Path.GetFullPath(LinkerAssembly), TypeMapBuildContract.LinkerIdentity,
                TypeMapBuildContract.LinkerVersion, ReceiptPath, Path.GetFullPath(OutputDirectory), Configuration, files, assemblies, effectiveLink);
            var content = JsonSerializer.Serialize(signature);
            var changed = !File.Exists(SignaturePath) || File.ReadAllText(SignaturePath) != content;
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs($"AtomUI TypeMap link inputs: signatureExists={File.Exists(SignaturePath)}, contentChanged={changed}, receiptExists={File.Exists(ReceiptPath)}.", null, nameof(PrepareTypeMapLinkTask), MessageImportance.High));
            if (changed || !File.Exists(ReceiptPath))
            {
                if (File.Exists(LinkSemaphore))
                {
                    File.Delete(LinkSemaphore);
                }

                if (File.Exists(ReceiptPath))
                {
                    File.Delete(ReceiptPath);
                }

                if (File.Exists(ReceiptPath + ".binding"))
                {
                    File.Delete(ReceiptPath + ".binding");
                }
            }
            if (changed)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(SignaturePath))!);
                File.WriteAllText(SignaturePath, content);
            }
            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or BadImageFormatException or ArgumentException or JsonException)
        { return Fail("ATOMUIREG006", error); }
    }
}

public sealed class VerifyTypeMapReceiptTask : RegistrationBuildTask
{
    public string ReceiptPath { get; set; } = string.Empty;
    public string SignaturePath { get; set; } = string.Empty;
    public bool BindReceipt { get; set; }
    public bool RequireConsumedAssemblies { get; set; }
    public ITaskItem[] ConsumedAssemblies { get; set; } = [];

    public override bool Execute()
    {
        try
        {
            if (RequireConsumedAssemblies && ConsumedAssemblies.Length == 0)
            {
                throw new InvalidDataException("The TypeMap consumption gate received no actual AOT/bundle input assemblies.");
            }

            if (!File.Exists(ReceiptPath))
            {
                throw new InvalidDataException($"Missing AtomUI TypeMap output receipt '{ReceiptPath}'. The required converter did not complete.");
            }

            var signature = JsonSerializer.Deserialize<TypeMapLinkSignature>(File.ReadAllText(SignaturePath))
                ?? throw new InvalidDataException("Invalid TypeMap input signature.");
            if (signature.ReceiptPath != ReceiptPath || signature.Capability != TypeMapBuildContract.Capability)
            {
                throw new InvalidDataException("TypeMap receipt/configuration mismatch.");
            }

            foreach (var file in signature.Files)
            {
                if (TypeMapBuildContract.Hash(file.Path) != file.Hash)
                {
                    throw new InvalidDataException($"TypeMap build input changed after linking: '{file.Path}'.");
                }
            }

            foreach (var assembly in signature.Assemblies)
            {
                if (TypeMapBuildContract.Hash(assembly.Path) != assembly.Hash)
                {
                    throw new InvalidDataException($"TypeMap assembly input changed after linking: '{assembly.Path}'.");
                }
            }

            using var receipt = JsonDocument.Parse(File.ReadAllText(ReceiptPath));
            var root = receipt.RootElement;
            void Require(string field, string expected)
            {
                if (root.GetProperty(field).GetString() != expected)
                {
                    throw new InvalidDataException($"TypeMap receipt '{field}' mismatch.");
                }
            }
            Require("status", "output-verified"); Require("capability", signature.Capability);
            Require("toolIdentity", signature.BackendIdentity); Require("toolSha256", signature.BackendHash);
            Require("linkerIdentity", signature.LinkerIdentity); Require("linkerVersion", signature.LinkerVersion);
            if (root.GetProperty("abi").GetInt32() != 1)
            {
                throw new InvalidDataException("Unsupported TypeMap receipt ABI.");
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var artifact in root.GetProperty("assemblies").EnumerateArray())
            {
                var identity = artifact.GetProperty("identity").GetString()!;
                if (!seen.Add(identity))
                {
                    throw new InvalidDataException($"Duplicate TypeMap output identity '{identity}'.");
                }

                var input = signature.Assemblies.Single(a => a.Identity == identity);
                if (input.Hash != artifact.GetProperty("inputSha256").GetString())
                {
                    throw new InvalidDataException($"Stale TypeMap input hash '{identity}'.");
                }

                var output = Path.Combine(signature.OutputDirectory, Path.GetFileName(input.Path));
                var expectedHash = artifact.GetProperty("outputSha256").GetString();
                if (TypeMapBuildContract.Hash(output) != expectedHash || AssemblyName.GetAssemblyName(output).FullName != identity)
                {
                    throw new InvalidDataException($"TypeMap linked output was replaced: '{output}'.");
                }

                if (artifact.TryGetProperty("outputSymbolsSha256", out var symbolHash) && symbolHash.ValueKind == JsonValueKind.String &&
                    TypeMapBuildContract.Hash(Path.ChangeExtension(output, ".pdb")) != symbolHash.GetString())
                {
                    throw new InvalidDataException($"TypeMap linked symbols were replaced: '{output}'.");
                }

                if (ConsumedAssemblies.Length != 0)
                {
                    var consumed = ConsumedAssemblies.Where(i => Path.GetFileName(i.ItemSpec) == Path.GetFileName(input.Path)).ToArray();
                    if (consumed.Length != 1 || TypeMapBuildContract.Hash(consumed[0].ItemSpec) != expectedHash)
                    {
                        throw new InvalidDataException($"AOT/bundle input does not match the verified TypeMap output '{identity}'.");
                    }
                }
            }
            var slots = new HashSet<string>(StringComparer.Ordinal);
            foreach (var slot in root.GetProperty("accessors").EnumerateArray())
            {
                var assembly = slot.GetProperty("assembly").GetString()!;
                if (!seen.Contains(assembly) || !slots.Add(assembly + "::" + slot.GetProperty("method").GetString()) ||
                    slot.GetProperty("bodySha256").GetString()?.Length != 64 || string.IsNullOrWhiteSpace(slot.GetProperty("group").GetString()))
                {
                    throw new InvalidDataException("Invalid TypeMap verified accessor inventory.");
                }
            }
            var binding = TypeMapBuildContract.Hash(SignaturePath) + "\n" + TypeMapBuildContract.Hash(ReceiptPath);
            if (BindReceipt)
            {
                File.WriteAllText(ReceiptPath + ".binding", binding);
            }
            else if (!File.Exists(ReceiptPath + ".binding") || File.ReadAllText(ReceiptPath + ".binding") != binding)
            {
                throw new InvalidDataException("TypeMap receipt does not belong to the current verified link invocation.");
            }

            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or BadImageFormatException or ArgumentException or
                                      InvalidOperationException or KeyNotFoundException or JsonException)
        { return Fail("ATOMUIREG007", error); }
    }
}

public sealed class ValidateRegistrationToolchainTask : RegistrationBuildTask
{
    public string LinkerAssembly { get; set; } = string.Empty;
    public string NativeCompiler { get; set; } = string.Empty;
    public string TargetFrameworkIdentifier { get; set; } = string.Empty;
    public string TargetFrameworkVersion { get; set; } = string.Empty;
    public override bool Execute()
    {
        try
        {
            if (RegistrationFrameworkPolicy.Classify(TargetFrameworkIdentifier, TargetFrameworkVersion) != RegistrationFrameworkFamily.OfficialTypeMap ||
                !RegistrationFrameworkPolicy.TryParseVersion(TargetFrameworkVersion, out var targetVersion))
            {
                throw new InvalidDataException(
                    $"Unsupported official TypeMap target framework '{TargetFrameworkIdentifier},Version={TargetFrameworkVersion}'. Expected .NETCoreApp 10.0 or later.");
            }

            if (!string.IsNullOrEmpty(LinkerAssembly) && !TypeMapBuildContract.IsSupportedDesktopLinker(
                    AssemblyName.GetAssemblyName(LinkerAssembly).FullName, TypeMapBuildContract.InformationalVersion(LinkerAssembly), targetVersion))
            {
                throw new InvalidDataException(
                    $"Unsupported actual ILLink binary '{LinkerAssembly}'. Expected official ILLink major {targetVersion.Major}, version at least {targetVersion.Major}.{targetVersion.Minor} and {TypeMapBuildContract.DesktopToolchainFloor}.");
            }

            if (!string.IsNullOrEmpty(NativeCompiler))
            {
                using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(NativeCompiler)
                {
                    ArgumentList = { "--version" }, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
                }) ?? throw new InvalidDataException($"Cannot start actual NativeAOT compiler '{NativeCompiler}'.");
                var version = process.StandardOutput.ReadToEnd().Trim();
                var errors = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0 || !TypeMapBuildContract.IsSupportedDesktopToolchain(version, targetVersion))
                {
                    throw new InvalidDataException(
                        $"Unsupported actual NativeAOT compiler '{NativeCompiler}': {version} {errors}. Expected ILCompiler major {targetVersion.Major}, version at least {targetVersion.Major}.{targetVersion.Minor} and {TypeMapBuildContract.DesktopToolchainFloor}.");
                }
            }
            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or BadImageFormatException or ArgumentException or System.ComponentModel.Win32Exception)
        { return Fail("ATOMUIREG006", error); }
    }
}
