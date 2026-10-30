using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace AtomUI.Generator.Localization.Catalog;

internal readonly struct CatalogKey : IEquatable<CatalogKey>
{
    internal CatalogKey(string moduleId, string fileId)
    {
        ModuleId = moduleId;
        FileId = fileId;
    }

    internal string ModuleId { get; }

    internal string FileId { get; }

    public bool Equals(CatalogKey other)
    {
        return string.Equals(ModuleId, other.ModuleId, StringComparison.Ordinal) &&
               string.Equals(FileId, other.FileId, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is CatalogKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (StringComparer.Ordinal.GetHashCode(ModuleId) * 397) ^
                   StringComparer.Ordinal.GetHashCode(FileId);
        }
    }

    public override string ToString()
    {
        return $"{ModuleId}:{FileId}";
    }
}

internal sealed class LanguageCatalogInfo
{
    internal LanguageCatalogInfo(
        string moduleId,
        string metadataName,
        string @namespace,
        string typeName,
        ImmutableArray<LanguageCatalogUnitInfo> units,
        Location location)
    {
        ModuleId = moduleId;
        MetadataName = metadataName;
        Namespace = @namespace;
        TypeName = typeName;
        Units = units;
        Location = location;
        Key = new CatalogKey(moduleId, metadataName);
    }

    internal string ModuleId { get; }

    internal string MetadataName { get; }

    internal string Namespace { get; }

    internal string TypeName { get; }

    internal ImmutableArray<LanguageCatalogUnitInfo> Units { get; }

    internal Location Location { get; }

    internal CatalogKey Key { get; }

    internal string CatalogId => Key.ToString();
}

internal sealed class LanguageCatalogUnitInfo
{
    internal LanguageCatalogUnitInfo(string key, Location location, int enumValue = 0)
    {
        Key = key;
        Location = location;
        EnumValue = enumValue;
    }

    internal string Key { get; }

    internal int EnumValue { get; }

    internal Location Location { get; }
}

internal sealed class LanguageCatalogParseResult
{
    internal LanguageCatalogParseResult(
        LanguageCatalogInfo? catalog,
        ImmutableArray<Diagnostic> diagnostics)
    {
        Catalog = catalog;
        Diagnostics = diagnostics;
    }

    internal LanguageCatalogInfo? Catalog { get; }

    internal ImmutableArray<Diagnostic> Diagnostics { get; }
}
