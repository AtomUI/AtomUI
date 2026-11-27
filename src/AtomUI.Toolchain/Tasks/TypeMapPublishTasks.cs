using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

internal static class TypeMapBuildContract
{
    internal const string Capability = OfficialTypeMapToolchain.Capability;
    // Desktop trimming consumes the official TypeMap contract with a toolchain matching the target framework.
    // TypeMap was introduced in .NET 10; servicing versions do not select its availability.
    internal static readonly Version MinimumTypeMapVersion = OfficialTypeMapToolchain.MinimumVersion;
    private const string MicrosoftPublicKeyToken = "31BF3856AD364E35";

    internal static bool IsSupportedDesktopToolchain(string informationalVersion, Version targetFrameworkVersion)
    {
        return OfficialTypeMapToolchain.TryParseVersion(informationalVersion, out var version) &&
               version.Major == targetFrameworkVersion.Major &&
               version >= new Version(targetFrameworkVersion.Major, targetFrameworkVersion.Minor) &&
               version >= MinimumTypeMapVersion;
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

/// <summary>
/// Base class of the registration build tasks. It holds the MSBuild <c>ITask</c> members and provides
/// <see cref="Fail" /> for reporting a failed task.
/// </summary>
public abstract class RegistrationBuildTask : ITask
{
    /// <summary>
    /// Build engine that receives the errors and messages a task logs. MSBuild or the task host assigns it
    /// before <see cref="Execute" /> runs.
    /// </summary>
    public IBuildEngine BuildEngine { get; set; } = null!;
    /// <summary>
    /// Host object of the MSBuild <c>ITask</c> contract. No task in the toolchain reads it.
    /// </summary>
    public ITaskHost HostObject { get; set; } = null!;
    /// <summary>
    /// Runs the task.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the task succeeded. <see langword="false" /> after the failure was logged
    /// through <see cref="BuildEngine" />, for example by returning <see cref="Fail" />.
    /// </returns>
    public abstract bool Execute();
    /// <summary>
    /// Logs the message of <paramref name="error" /> as an MSBuild error with the subcategory
    /// <c>Registration</c> and returns <see langword="false" />, so an <see cref="Execute" /> override can
    /// return it directly.
    /// </summary>
    /// <param name="code">
    /// Diagnostic code, such as <c>ATOMUIREG006</c>.
    /// </param>
    /// <param name="error">
    /// Exception whose message becomes the error text.
    /// </param>
    /// <returns>
    /// Always <see langword="false" />.
    /// </returns>
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

/// <summary>
/// Freezes the inputs of a TypeMap link in a signature file and clears the previous link result when
/// the inputs changed. It does not run the link.
/// </summary>
public sealed class PrepareTypeMapLinkTask : RegistrationBuildTask
{
    // Retained for older build assets. SDK version strings do not determine TypeMap support.
    /// <summary>
    /// SDK version string that older build assets still pass. This task does not read it.
    /// </summary>
    public string SdkVersion { get; set; } = string.Empty;
    /// <summary>
    /// Path of the WebAssembly SDK tasks assembly. When set, the file is hashed into the signature as a
    /// link input, so a change to it discards the previous link result. An empty value adds no file.
    /// </summary>
    public string WasmSdkTasksPath { get; set; } = string.Empty;
    /// <summary>
    /// Path of the ILLink backend adapter assembly. Its directory locates the backend files, and its full
    /// path, identity and hash are recorded in the signature.
    /// </summary>
    public string BackendAssembly { get; set; } = string.Empty;
    /// <summary>
    /// Path of the ILLink executable (<c>illink</c>). It must be an official ILLink binary of at least
    /// <c>OfficialTypeMapToolchain.MinimumVersion</c> with TypeMap support, otherwise the task fails. Its
    /// directory also supplies the linker support files that are hashed into the signature.
    /// </summary>
    public string LinkerAssembly { get; set; } = string.Empty;
    /// <summary>
    /// Capability string of the TypeMap contract. The task fails unless it equals
    /// <c>TypeMapBuildContract.Capability</c>, which is also the default, and the receipt must report it.
    /// </summary>
    public string Capability { get; set; } = TypeMapBuildContract.Capability;
    /// <summary>
    /// Absolute path of the receipt that the TypeMap converter writes when it completes. It is recorded in
    /// the signature. When the inputs changed or the receipt is missing, the task deletes the receipt and
    /// its <c>.binding</c> file.
    /// </summary>
    public string ReceiptPath { get; set; } = string.Empty;
    /// <summary>
    /// Path of the JSON file that lists the link inputs with their hashes. The task rewrites it when its
    /// content differs from the current inputs.
    /// </summary>
    public string SignaturePath { get; set; } = string.Empty;
    /// <summary>
    /// Path of a semaphore file that this task never creates. It is deleted when the inputs changed or the
    /// receipt is missing.
    /// </summary>
    public string LinkSemaphore { get; set; } = string.Empty;
    /// <summary>
    /// Directory that receives the linked assemblies. This task only records its full path in the
    /// signature. <see cref="VerifyTypeMapReceiptTask" /> looks there for the linked output of each
    /// assembly it finds in the receipt, under the same file name.
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;
    /// <summary>
    /// Build configuration. This task only records it in the signature.
    /// </summary>
    public string Configuration { get; set; } = string.Empty;
    /// <summary>
    /// Link inputs other than assemblies, each with a <c>Kind</c> metadata value that names its role.
    /// <c>Reference</c> items are reference assemblies; one whose full identity equals an assembly in
    /// <see cref="Assemblies" /> is dropped from the hashed files and from the link configuration.
    /// <c>Tool</c> items are passed with <see cref="LinkerAssembly" /> to the link configuration to resolve
    /// the files it contributes to the hashed list. Items of any other kind are only hashed.
    /// </summary>
    public ITaskItem[] Inputs { get; set; } = [];
    /// <summary>
    /// Assemblies to link. The path, full identity, hash and the <c>TrimMode</c> and <c>IsTrimmable</c>
    /// metadata of each item are recorded in the signature, sorted by identity.
    /// </summary>
    public ITaskItem[] Assemblies { get; set; } = [];
    /// <summary>
    /// Scalar ILLink options. The item spec is the option name and the <c>Value</c> metadata is its value.
    /// The names must be unique or the task fails. The value of <c>NoWarn</c> is reduced to its IL warning
    /// codes.
    /// </summary>
    public ITaskItem[] LinkOptions { get; set; } = [];
    /// <summary>
    /// Items of the ILLink input collections, in the order ILLink receives them. The <c>Kind</c> metadata
    /// names the collection: <c>RootAssemblyNames</c>, <c>FeatureSettings</c>, <c>CustomData</c>,
    /// <c>CustomSteps</c>, <c>RootDescriptorFiles</c> or <c>KeepMetadata</c>. Any other kind fails the task.
    /// </summary>
    public ITaskItem[] LinkItems { get; set; } = [];

    /// <summary>
    /// Validates the capability, the receipt path and the linker, writes the signature of the current
    /// inputs, and deletes the semaphore, the receipt and its <c>.binding</c> file when the inputs changed
    /// or the receipt is missing.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when the signature is current, whether it was rewritten or left as it was.
    /// <see langword="false" /> after an invalid input or a file access error is logged as
    /// <c>ATOMUIREG006</c>.
    /// </returns>
    public override bool Execute()
    {
        try
        {
            if (Capability != TypeMapBuildContract.Capability)
            {
                throw new InvalidDataException($"Unsupported backend capability '{Capability}'.");
            }

            if (!Path.IsPathFullyQualified(ReceiptPath))
            {
                throw new InvalidDataException("AtomUITypeMapReceipt must be an absolute path.");
            }

            var linkerIdentity = AssemblyName.GetAssemblyName(LinkerAssembly).FullName!;
            var linkerVersion = TypeMapBuildContract.InformationalVersion(LinkerAssembly);
            if (!OfficialTypeMapToolchain.IsSupportedLinker(linkerIdentity, linkerVersion))
            {
                throw new InvalidDataException($"Unsupported actual ILLink binary '{LinkerAssembly}' ({linkerVersion}). Expected official ILLink {OfficialTypeMapToolchain.MinimumVersion} or later with TypeMap support.");
            }

            var toolDirectory = Path.GetDirectoryName(BackendAssembly)!;
            var linkerDirectory = Path.GetDirectoryName(LinkerAssembly)!;
            var linkerTargets = Path.GetFullPath(Path.Combine(linkerDirectory, "../../build/Microsoft.NET.ILLink.targets"));
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
                .Concat(new[] { LinkerAssembly, linkerTargets, Path.Combine(linkerDirectory, "Mono.Cecil.dll"), Path.Combine(linkerDirectory, "illink.deps.json"),
                    Path.Combine(linkerDirectory, "illink.runtimeconfig.json"), WasmSdkTasksPath, typeof(PrepareTypeMapLinkTask).Assembly.Location })
                .Where(p => !string.IsNullOrWhiteSpace(p)).Select(Path.GetFullPath).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
                .Select(p => new TypeMapFile(p, TypeMapBuildContract.Hash(p))).ToArray();
            var signature = new TypeMapLinkSignature(Capability, Path.GetFullPath(BackendAssembly), AssemblyName.GetAssemblyName(BackendAssembly).FullName!,
                TypeMapBuildContract.Hash(BackendAssembly), Path.GetFullPath(LinkerAssembly), linkerIdentity,
                linkerVersion, ReceiptPath, Path.GetFullPath(OutputDirectory), Configuration, files, assemblies, effectiveLink);
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

/// <summary>
/// Checks that the TypeMap receipt belongs to the current link inputs and that the linked outputs were
/// not replaced afterwards, then binds the receipt to the signature or verifies the existing binding.
/// </summary>
public sealed class VerifyTypeMapReceiptTask : RegistrationBuildTask
{
    /// <summary>
    /// Path of the receipt that the TypeMap converter wrote. It must exist, and its path and capability
    /// must match the signature.
    /// </summary>
    public string ReceiptPath { get; set; } = string.Empty;
    /// <summary>
    /// Path of the signature written by <see cref="PrepareTypeMapLinkTask" />. It is the baseline for every
    /// check, and its hash together with the hash of the receipt at <see cref="ReceiptPath" /> forms the
    /// binding.
    /// </summary>
    public string SignaturePath { get; set; } = string.Empty;
    /// <summary>
    /// Controls how the <c>.binding</c> file next to the receipt is treated. When
    /// <see langword="true" />, the task writes it from the hashes of the signature and the receipt. When
    /// <see langword="false" />, the file must already exist with that content, otherwise the task fails.
    /// </summary>
    public bool BindReceipt { get; set; }
    /// <summary>
    /// When <see langword="true" />, the task fails when <see cref="ConsumedAssemblies" /> is empty.
    /// </summary>
    public bool RequireConsumedAssemblies { get; set; }
    /// <summary>
    /// Assembly files that the AOT or bundle step takes as its input. When not empty, every output listed
    /// in the receipt must match exactly one item with the same file name and the recorded output hash.
    /// </summary>
    public ITaskItem[] ConsumedAssemblies { get; set; } = [];

    /// <summary>
    /// Checks that the recorded inputs, assemblies and linked outputs still have the hashes recorded in the
    /// signature and the receipt, that the receipt names the expected status, capability, backend and
    /// linker, and that the consumed assemblies match the outputs. Then binds the receipt to the signature
    /// or verifies the existing binding.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when every check passed. <see langword="false" /> after a mismatch, a file
    /// access error or malformed receipt data is logged as <c>ATOMUIREG007</c>.
    /// </returns>
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

/// <summary>
/// Checks that the target framework, the ILLink binary and the NativeAOT compiler are the official ones
/// that support the TypeMap contract.
/// </summary>
public sealed class ValidateRegistrationToolchainTask : RegistrationBuildTask
{
    /// <summary>
    /// Path of the ILLink binary. When set, it must be the official <c>illink</c> with the Microsoft key,
    /// at a version that has the major of the target framework and is at least the target framework
    /// version and <c>TypeMapBuildContract.MinimumTypeMapVersion</c>, otherwise the task fails. When empty,
    /// the linker is not checked.
    /// </summary>
    public string LinkerAssembly { get; set; } = string.Empty;
    /// <summary>
    /// Path of the NativeAOT compiler. When set, the task starts it with <c>--version</c> and requires a
    /// version that has the major of the target framework and is at least the target framework version and
    /// <c>TypeMapBuildContract.MinimumTypeMapVersion</c>, otherwise the task fails. When empty, the
    /// compiler is not checked.
    /// </summary>
    public string NativeCompiler { get; set; } = string.Empty;
    /// <summary>
    /// Identifier of the target framework, such as <c>.NETCoreApp</c>. Together with
    /// <see cref="TargetFrameworkVersion" /> it must classify as the official TypeMap framework, otherwise
    /// the task fails.
    /// </summary>
    public string TargetFrameworkIdentifier { get; set; } = string.Empty;
    /// <summary>
    /// Version of the target framework, such as <c>v10.0</c>. Together with
    /// <see cref="TargetFrameworkIdentifier" /> it must classify as the official TypeMap framework. Its
    /// major and minor version are the baseline for the linker and compiler checks.
    /// </summary>
    public string TargetFrameworkVersion { get; set; } = string.Empty;
    /// <summary>
    /// Checks the target framework, then the linker when <see cref="LinkerAssembly" /> is set, then the
    /// NativeAOT compiler when <see cref="NativeCompiler" /> is set. The compiler check starts the compiler
    /// with <c>--version</c>.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> when every check passed or was skipped. <see langword="false" /> after an
    /// unsupported tool, or a failure to read or start it, is logged as <c>ATOMUIREG006</c>.
    /// </returns>
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
                    $"Unsupported actual ILLink binary '{LinkerAssembly}'. Expected official ILLink major {targetVersion.Major}, version at least {targetVersion.Major}.{targetVersion.Minor} and {TypeMapBuildContract.MinimumTypeMapVersion}.");
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
                        $"Unsupported actual NativeAOT compiler '{NativeCompiler}': {version} {errors}. Expected ILCompiler major {targetVersion.Major}, version at least {targetVersion.Major}.{targetVersion.Minor} and {TypeMapBuildContract.MinimumTypeMapVersion}.");
                }
            }
            return true;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or BadImageFormatException or ArgumentException or System.ComponentModel.Win32Exception)
        { return Fail("ATOMUIREG006", error); }
    }
}
