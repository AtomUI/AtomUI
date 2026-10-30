using AtomUI.Startup.Data;
using AtomUI.Theme.Definitions.Compiled;

namespace AtomUI.Generator;

internal static class StartupThemeLinker
{
    internal static SortedDictionary<string, string> Validate(IEnumerable<StartupReferenceData> references,
        string ownId, CompiledThemeManifest? ownTheme, ThemeBuildContracts? contracts,
        IReadOnlyList<StartupPackageData> packages, string? ownAssemblyName = null)
    {
        var manifests = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var origins = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var reference in references)
        {
            foreach (var theme in reference.Themes)
            {
                if (theme.Version != 1)
                {
                    throw new InvalidDataException("Unsupported theme manifest from '" + reference.AssemblyName + "'.");
                }
                Add(theme.PackageId, theme.Data, reference.AssemblyName);
            }
        }
        if (ownTheme is not null)
        {
            Add(ownId, ownTheme.Encode(), ownAssemblyName ?? ownId);
        }
        foreach (var pair in manifests)
        {
            var manifest = StartupManifestDecoder.Read("Compiled theme package", pair.Key, origins[pair.Key],
                pair.Value, CompiledThemeManifest.Decode);
            foreach (var requirement in manifest.Requirements)
            {
                var context = "package '" + pair.Key + "', theme '" + requirement.ThemeId + "', " + requirement.Scope + " '" +
                    requirement.Catalog + ":" + requirement.ControlId + "/" + requirement.TokenName + "'";
                if (contracts is null)
                {
                    throw new InvalidDataException(context + " has no supported build contracts.");
                }
                if (requirement.Scope != "global")
                {
                    var key = requirement.Catalog + "\u001f" + requirement.ControlId;
                    var control = packages.SelectMany(package => package.Controls.Select(value => (Package: package, Control: value)))
                        .SingleOrDefault(value => value.Control.HasTokenDescriptor && value.Control.Catalog == requirement.Catalog && value.Control.Id == requirement.ControlId);
                    if (!contracts.Owners.TryGetValue(key, out var owner) || control.Control is null ||
                        SimpleAssembly(owner.OwnerAssembly) != requirement.OwnerAssemblyName ||
                        Normalize(owner.OwnerType) != Normalize(requirement.OwnerMetadataName) ||
                        (control.Control.AssemblyName ?? control.Package.AssemblyName) != requirement.OwnerAssemblyName ||
                        Normalize(control.Control.TypeName) != Normalize(requirement.OwnerMetadataName))
                    {
                        throw new InvalidDataException(context + " has an unavailable or incompatible Control owner.");
                    }
                }
                if (requirement.Scope == "control")
                {
                    continue;
                }
                var tokenKey = requirement.Scope == "own" ? requirement.Catalog + "\u001f" + requirement.ControlId + "\u001f" + requirement.TokenName : "\u001f\u001f" + requirement.TokenName;
                if (!contracts.Tokens.TryGetValue(tokenKey, out var token) || token.Contract != 1 ||
                    token.Contract != requirement.ValueContractVersion || token.Stage != requirement.Stage ||
                    ThemeLiteralCodec.NormalizeType(token.ValueType) != ThemeLiteralCodec.NormalizeType(requirement.ValueType) ||
                    Normalize(token.KeyType) != Normalize(requirement.ResourceKeyType) || token.KeyMember != requirement.ResourceKeyMember)
                {
                    throw new InvalidDataException(context + " has an unavailable or incompatible Token type, resource key, stage or literal capability.");
                }
            }
        }
        return manifests;

        void Add(string id, string data, string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(id) || manifests.ContainsKey(id))
            {
                throw new InvalidDataException("Duplicate or empty compiled theme package identity '" + id + "'.");
            }
            manifests.Add(id, data);
            origins.Add(id, assemblyName);
        }
    }

    private static string SimpleAssembly(string identity) => identity.Split(',')[0];
    private static string Normalize(string name) => name.Replace("global::", "");
}
