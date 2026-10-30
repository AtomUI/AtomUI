using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Text.Json;

namespace AtomUI.Build.Tasks.Registration;

internal static class ConditionalRegistrationOutputVerifier
{
    internal static void VerifyConsumed(string inputsPath, string receiptPath, string publishDirectory, string linkerAssembly)
    {
        using var receipt = JsonDocument.Parse(File.ReadAllBytes(receiptPath));
        var root = receipt.RootElement;
        var inputsBytes = File.ReadAllBytes(inputsPath);
        using var inputs = JsonDocument.Parse(inputsBytes);
        ValidateAdditionalInputs(inputs.RootElement);
        var analysis = inputs.RootElement.GetProperty("analysisReportPath").GetString()!;
        if (root.GetProperty("format").GetInt32() != 1 || root.GetProperty("stage").GetString() is not ("verified" or "consumed") ||
            root.GetProperty("inputHash").GetString() != Hash(inputsBytes) || root.GetProperty("analysisHash").GetString() != Hash(File.ReadAllBytes(analysis)) ||
            root.GetProperty("linkerHash").GetString() != Hash(File.ReadAllBytes(linkerAssembly)))
        {
            throw new InvalidDataException("Registration output receipt does not match this publish invocation.");
        }
        var directory = Path.GetFullPath(publishDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var consumed = new List<object>();
        foreach (var output in root.GetProperty("outputs").EnumerateArray())
        {
            var relative = output.GetProperty("relativePath").GetString()!;
            var actual = Path.GetFullPath(Path.Combine(directory, relative));
            if (Path.IsPathFullyQualified(relative) || !actual.StartsWith(directory,
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            {
                throw new InvalidDataException("Registration receipt contains a path outside the SDK publish directory.");
            }
            var expectedHash = output.GetProperty("sha256").GetString();
            if (!File.Exists(actual) || Hash(File.ReadAllBytes(actual)) != expectedHash)
            {
                throw new InvalidDataException("The SDK did not consume the verified registration output: " + relative);
            }
            consumed.Add(new { path = actual, sha256 = expectedHash });
        }
        if (consumed.Count == 0)
        {
            throw new InvalidDataException("Registration receipt contains no verified output to consume.");
        }
        RegistrationFiles.ReplaceText(receiptPath, JsonSerializer.Serialize(new
        {
            format = 1, stage = "consumed",
            inputHash = root.GetProperty("inputHash").GetString(),
            analysisHash = root.GetProperty("analysisHash").GetString(),
            linkerHash = root.GetProperty("linkerHash").GetString(),
            outputs = root.GetProperty("outputs"), consumer = "sdk-publish-copy", consumed
        }));
    }

    internal static void Verify(string inputsPath, string outputDirectory, string receiptPath, string linkerAssembly)
    {
        if (File.Exists(receiptPath))
        {
            File.Delete(receiptPath);
        }
        var inputsBytes = File.ReadAllBytes(inputsPath);
        using var inputs = JsonDocument.Parse(inputsBytes);
        ValidateAdditionalInputs(inputs.RootElement);
        var inputHash = Hash(inputsBytes);
        var core = inputs.RootElement.GetProperty("coreAssemblyIdentity").GetString()!;
        var reportPath = inputs.RootElement.GetProperty("analysisReportPath").GetString()!;
        var reportBytes = File.ReadAllBytes(reportPath);
        using var report = JsonDocument.Parse(reportBytes);
        var root = report.RootElement;
        RegistrationContractJson.ValidateUniqueFields(root);
        RegistrationContractJson.Fields(root, "format", "stage", "inputHash", "backend", "runtimeModeNormalization", "result");
        RegistrationContractJson.Fields(root.GetProperty("result"), "Groups", "Observations");
        if (root.GetProperty("format").GetInt32() != 1 || root.GetProperty("stage").GetString() != "analyzed-materialized" ||
            root.GetProperty("inputHash").GetString() != inputHash ||
            root.GetProperty("backend").GetString() != TypeMapBuildContract.InformationalVersion(linkerAssembly) ||
            root.GetProperty("runtimeModeNormalization").GetString() != "atomui.trimmed-switch.v1")
        {
            throw new InvalidDataException("Registration analysis report does not belong to these inputs and linker.");
        }
        var groups = root.GetProperty("result").GetProperty("Groups").EnumerateArray()
            .ToDictionary(group => group.GetProperty("Identity").GetString()!, StringComparer.Ordinal);
        var manifests = new List<ConditionalRegistrationManifest>();
        foreach (var item in inputs.RootElement.GetProperty("assemblies").EnumerateArray())
        {
            var input = ConditionalRegistrationMetadata.Read(item.GetProperty("path").GetString()!, core);
            if (input.ContentHash != item.GetProperty("sha256").GetString() || input.AssemblyIdentity != item.GetProperty("assemblyIdentity").GetString())
            {
                throw new InvalidDataException("Registration input was changed after analysis.");
            }
            if (input.Records.Count != 0)
            {
                manifests.Add(ConditionalRegistrationManifest.Parse(input));
            }
        }
        if (groups.Count != manifests.Count || manifests.Any(manifest => !groups.ContainsKey(manifest.Package.Group.QualifiedName)))
        {
            throw new InvalidDataException("Registration analysis omitted or added package groups.");
        }
        var outputs = new List<VerifiedOutput>();
        var outputByIdentity = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var path in Directory.EnumerateFiles(outputDirectory, "*.dll", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            var metadata = ConditionalRegistrationMetadata.Read(path, core);
            if (metadata.Records.Count != 0 || !outputByIdentity.TryAdd(metadata.AssemblyIdentity, path))
            {
                throw new InvalidDataException("Linked output contains unconsumed candidate records or duplicate assembly identities.");
            }
            outputs.Add(new(Path.GetFullPath(path), Path.GetRelativePath(outputDirectory, path), metadata.AssemblyIdentity, metadata.ContentHash));
        }
        if (outputs.Count == 0)
        {
            throw new InvalidDataException("Registration output directory contains no managed assemblies.");
        }
        if (outputByIdentity.TryGetValue(core, out var linkedCore))
        {
            using var assembly = new ConditionalDispatchReader(linkedCore);
            assembly.VerifyTrimmedMode();
        }
        foreach (var manifest in manifests)
        {
            var group = groups[manifest.Package.Group.QualifiedName];
            RegistrationContractJson.Fields(group, "Identity", "Requested", "SelectedFragments");
            var selected = group.GetProperty("SelectedFragments").EnumerateArray().Select(value => value.GetString()!).ToArray();
            var selectedIds = selected.ToHashSet(StringComparer.Ordinal);
            var fragmentIds = manifest.Fragments.Select(fragment => fragment.Identity).ToHashSet(StringComparer.Ordinal);
            if (selectedIds.Count != selected.Length || !selectedIds.IsSubsetOf(fragmentIds))
            {
                throw new InvalidDataException("Registration analysis contains unknown or duplicate selected fragments.");
            }
            if (!group.GetProperty("Requested").GetBoolean())
            {
                if (selected.Length != 0)
                {
                    throw new InvalidDataException("An unrequested registration group selected fragments.");
                }
                continue;
            }
            if (!outputByIdentity.TryGetValue(manifest.Metadata.AssemblyIdentity, out var output))
            {
                throw new InvalidDataException("A requested registration package is missing from linked output.");
            }
            var entries = manifest.Fragments.Where(fragment => selectedIds.Contains(fragment.Identity)).ToArray();
            if (!entries.Select(fragment => fragment.Identity).SequenceEqual(selected))
            {
                throw new InvalidDataException("Registration analysis changed canonical fragment collection order.");
            }
            using var assembly = new ConditionalDispatchReader(output);
            if (assembly.Hash != outputs.Single(file => file.Identity == assembly.Identity).Hash)
            {
                throw new InvalidDataException("Linked output changed during registration verification.");
            }
            assembly.VerifyCalls(manifest.Package.Collect, [manifest.Package.CollectSelected], core);
            assembly.VerifyCalls(manifest.Package.CollectSelected, entries.Select(fragment => fragment.RegistrationMethod).ToArray(), core);
        }
        var receipt = JsonSerializer.Serialize(new
        {
            format = 1, stage = "verified", inputHash, analysisHash = Hash(reportBytes),
            linkerHash = Hash(File.ReadAllBytes(linkerAssembly)),
            outputs = outputs.Select(file => new { path = file.Path, relativePath = file.RelativePath, assemblyIdentity = file.Identity, sha256 = file.Hash })
        });
        RegistrationFiles.ReplaceText(receiptPath, receipt);
    }

    private static void ValidateAdditionalInputs(JsonElement inputs)
    {
        RegistrationContractJson.ValidateUniqueFields(inputs);
        RegistrationContractJson.Fields(inputs, "format", "coreAssemblyIdentity", "assemblies", "analysisReportPath", "additionalInputs");
        if (inputs.GetProperty("format").GetInt32() != 2 || inputs.GetProperty("additionalInputs").ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("Verified SDK publishing requires registration input transport format 2.");
        }
        var identities = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in inputs.GetProperty("additionalInputs").EnumerateArray())
        {
            RegistrationContractJson.Fields(item, "path", "sha256", "kind");
            var path = RegistrationContractJson.String(item, "path");
            var kind = RegistrationContractJson.String(item, "kind");
            var sha = RegistrationContractJson.String(item, "sha256");
            if (!Path.IsPathFullyQualified(path) || !identities.Add(path + "\n" + kind) ||
                kind is not ("root-descriptor" or "reference" or "custom-step" or "custom-step-file" or "tool" or "symbol" or "configuration" or "opaque") ||
                Hash(File.ReadAllBytes(path)) != sha)
            {
                throw new InvalidDataException("Registration additional input is invalid or changed after preparation: " + path);
            }
        }
    }

    private sealed record VerifiedOutput(string Path, string RelativePath, string Identity, string Hash);
}
