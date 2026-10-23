using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Branding;

public enum ThemeBrandRecipeKind : byte
{
    Complete,
    TokenOnly
}

public sealed class ThemeBrandRecipe
{
    public ThemeBrandRecipe(string id, string revision, string themeId, ThemeConfig config,
        IEnumerable<ThemeStyleAssetReference> assets, ThemeBrandRequirements requirements,
        ThemeBrandRecipeKind kind = ThemeBrandRecipeKind.Complete)
    {
        SchemaIdentifier.Validate(id, nameof(id));
        ThemeBrandIdentifiers.ValidateRevision(revision);
        ArgumentException.ThrowIfNullOrWhiteSpace(themeId);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentNullException.ThrowIfNull(requirements);
        if (themeId.Length > 256 || kind is not (ThemeBrandRecipeKind.Complete or ThemeBrandRecipeKind.TokenOnly))
        {
            throw new ArgumentException("The recipe theme or kind is invalid.");
        }
        var references = assets.ToArray();
        if (references.Any(static item => item is null) ||
            references.Select(static item => item.Id).Distinct(StringComparer.Ordinal).Count() != references.Length ||
            kind == ThemeBrandRecipeKind.TokenOnly && references.Length != 0)
        {
            throw new ArgumentException("A recipe requires distinct asset ids; token-only recipes cannot contain styles.", nameof(assets));
        }
        Id = id;
        Revision = revision;
        ThemeId = themeId;
        // The wire model contains Token input and asset references, never an executable factory.
        Config = new ThemeConfig(config.Inherit, config.Algorithms, config.Tokens, config.Controls);
        Assets = Array.AsReadOnly(references);
        Requirements = requirements;
        Kind = kind;
    }

    public int Version => 1;
    public string Id { get; }
    public string Revision { get; }
    public string ThemeId { get; }
    public ThemeConfig Config { get; }
    public IReadOnlyList<ThemeStyleAssetReference> Assets { get; }
    public ThemeBrandRequirements Requirements { get; }
    public ThemeBrandRecipeKind Kind { get; }
}
