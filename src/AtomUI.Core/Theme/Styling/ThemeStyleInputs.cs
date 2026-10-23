using AtomUI.Theme.Configuration;
using AtomUI.Theme.Branding;

namespace AtomUI.Theme.Styling;

internal sealed class ThemeStyleInputs : IEquatable<ThemeStyleInputs>
{
    internal static ThemeStyleInputs Empty { get; } = new([]);

    private ThemeStyleInputs(IReadOnlyList<ThemeStyleAsset> assets, ThemeBrandSelection? selection = null)
    {
        Assets = Array.AsReadOnly(assets.ToArray());
        Selection = selection;
    }

    internal IReadOnlyList<ThemeStyleAsset> Assets { get; }
    internal ThemeBrandSelection? Selection { get; }

    internal static ThemeStyleInputs Resolve(ThemeStyleInputs parent, ThemeConfig? config)
    {
        ArgumentNullException.ThrowIfNull(parent);
        if (config is null)
        {
            return parent;
        }
        var inherited = config.Inherit ? parent : Empty;
        var selection = config.BrandSelection ?? inherited.Selection;
        if (config.StyleAssets.Count == 0)
        {
            return Equals(selection, inherited.Selection) ? inherited : new ThemeStyleInputs(inherited.Assets, selection);
        }
        // A local replacement belongs to the local layer's order, after surviving parent assets.
        var localIds = config.StyleAssets.Select(static asset => asset.Id).ToHashSet(StringComparer.Ordinal);
        return new ThemeStyleInputs(inherited.Assets.Where(asset => !localIds.Contains(asset.Id))
            .Concat(config.StyleAssets).ToArray(), selection);
    }

    public bool Equals(ThemeStyleInputs? other)
    {
        return other is not null && Equals(Selection, other.Selection) && AssetsEqual(other);
    }

    internal bool AssetsEqual(ThemeStyleInputs other) => Assets.SequenceEqual(other.Assets);

    public override bool Equals(object? obj) => obj is ThemeStyleInputs other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Selection);
        foreach (var asset in Assets)
        {
            // Registry-owned immutable descriptors are canonical; equal ids must not hide a changed factory.
            hash.Add(asset);
        }
        return hash.ToHashCode();
    }
}
