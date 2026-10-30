using System.Security.Cryptography;
using System.Xml.Linq;
using AtomUI.Registration.Shared;
using Mono.Cecil;

namespace AtomUI.TypeMap.Linker.ConditionalBridge;

internal sealed record FrameworkBindingEvidence(
    string SourceIdentity,
    string TargetIdentity,
    string TargetSha256,
    string RuntimePackIdentity,
    string RuntimeListSha256);

internal sealed record FrameworkBindingSet(
    IReadOnlyDictionary<string, string> Bindings,
    IReadOnlyList<FrameworkBindingEvidence> Evidence);

/// <summary>Explicit net8 platform-reference bindings, confined to the SDK-resolved net10 runtime pack.</summary>
internal static class FrameworkBindingPolicy
{
    internal static void NormalizeReferences(IEnumerable<AssemblyDefinition> owners, IReadOnlyDictionary<string, string> bindings)
    {
        foreach (var owner in owners)
        {
            foreach (var reference in owner.MainModule.AssemblyReferences)
            {
                if (bindings.TryGetValue(reference.FullName, out var targetIdentity))
                {
                    var target = AssemblyNameReference.Parse(targetIdentity);
                    reference.Version = target.Version;
                }
            }
        }
    }

    internal static FrameworkBindingSet Create(
        string runtimePackDirectory,
        ConditionalInputSnapshot snapshot,
        IEnumerable<AssemblyDefinition> conditionalOwners)
    {
        if (!Path.IsPathFullyQualified(runtimePackDirectory))
        {
            throw new InvalidDataException("The bridge requires the absolute SDK-resolved runtime-pack directory.");
        }
        var pack = new DirectoryInfo(Path.GetFullPath(runtimePackDirectory));
        var admitted = pack.Parent is not null &&
            (pack.Name == "10.0.8" && pack.Parent.Name.StartsWith("Microsoft.NETCore.App.Runtime.", StringComparison.Ordinal) ||
             pack.Name == "10.0.10" && pack.Parent.Name == "Microsoft.NETCore.App.Runtime.Mono.browser-wasm");
        if (!admitted)
        {
            throw new InvalidDataException("The bridge requires an admitted SDK runtime pack (10.0.8, or Mono browser-wasm 10.0.10).");
        }
        var listPath = snapshot.Format == 2
            ? (snapshot.AdditionalInputs ?? []).Single(input => input.Kind == "configuration" &&
                Path.GetFileName(input.Path) == "RuntimeList.xml").Path
            : Path.Combine(pack.FullName, "data", "RuntimeList.xml");
        var bytes = File.ReadAllBytes(listPath);
        using var stream = new MemoryStream(bytes, writable: false);
        var list = XDocument.Load(stream).Root;
        if (list?.Name != "FileList" || (string?)list.Attribute("TargetFrameworkIdentifier") != ".NETCoreApp" ||
            (string?)list.Attribute("TargetFrameworkVersion") != "10.0" ||
            (string?)list.Attribute("FrameworkName") != "Microsoft.NETCore.App")
        {
            throw new InvalidDataException("Unexpected target FrameworkReference runtime-pack manifest.");
        }
        var managed = list.Elements("File").Where(file => (string?)file.Attribute("Type") == "Managed")
            .ToLookup(file => (string?)file.Attribute("AssemblyName"), StringComparer.Ordinal);
        var inputs = snapshot.Assemblies.ToDictionary(input => input.AssemblyIdentity, StringComparer.Ordinal);
        var bindings = new Dictionary<string, string>(StringComparer.Ordinal);
        var evidence = new List<FrameworkBindingEvidence>();
        var sources = conditionalOwners.SelectMany(owner => owner.MainModule.AssemblyReferences)
            .DistinctBy(reference => reference.FullName, StringComparer.Ordinal);
        foreach (var source in sources)
        {
            if (inputs.ContainsKey(source.FullName))
            {
                continue;
            }
            // This is not general assembly-version unification. Only the original net8 platform
            // identity may bind to the same signed assembly in the verified target runtime pack.
            var entries = managed[source.Name].Take(2).ToArray();
            if (source.Version != new Version(8, 0, 0, 0) || entries.Length != 1)
            {
                continue;
            }
            var entry = entries[0];
            var relativePath = (string?)entry.Attribute("Path") ?? throw new InvalidDataException("Missing runtime-pack asset path.");
            var targetPath = Path.GetFullPath(Path.Combine(pack.FullName, relativePath));
            if (!targetPath.StartsWith(pack.FullName + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Runtime-pack asset escapes its verified package.");
            }
            if (snapshot.Format == 2)
            {
                var targetIdentity = new AssemblyNameReference(source.Name, new Version(10, 0, 0, 0))
                {
                    Culture = source.Culture,
                    PublicKeyToken = source.PublicKeyToken
                }.FullName;
                if (!inputs.TryGetValue(targetIdentity, out var frozenTarget))
                {
                    throw new InvalidDataException("The frozen SDK framework input is missing: " + targetIdentity);
                }
                // The SDK preparation boundary froze the resolved pack's manifest and implementations.
                // The transformer consumes those bytes, never mutable package-cache originals.
                targetPath = frozenTarget.Path;
            }
            var targetBytes = File.ReadAllBytes(targetPath);
            using var targetStream = new MemoryStream(targetBytes, writable: false);
            using var target = AssemblyDefinition.ReadAssembly(targetStream);
            var name = target.Name;
            if (name.Name != source.Name || name.Version != new Version(10, 0, 0, 0) ||
                (name.Culture ?? string.Empty) != (source.Culture ?? string.Empty) ||
                source.PublicKeyToken.Length != 8 || !source.PublicKeyToken.SequenceEqual(name.PublicKeyToken) ||
                Convert.ToHexStringLower(name.PublicKeyToken) != (string?)entry.Attribute("PublicKeyToken") ||
                name.Version.ToString() != (string?)entry.Attribute("AssemblyVersion"))
            {
                throw new InvalidDataException("The source reference does not match the signed target framework asset: " + source.FullName);
            }
            var hash = Convert.ToHexStringLower(SHA256.HashData(targetBytes));
            if (!inputs.TryGetValue(name.FullName, out var input) || input.Sha256 != hash)
            {
                throw new InvalidDataException("The SDK input snapshot does not contain the exact runtime-pack implementation: " + name.FullName);
            }
            bindings.Add(source.FullName, name.FullName);
            evidence.Add(new(source.FullName, name.FullName, hash, pack.Parent!.Name + "/" + pack.Name,
                Convert.ToHexStringLower(SHA256.HashData(bytes))));
        }
        return new(bindings, evidence);
    }
}
