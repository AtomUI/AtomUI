using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// The immutable result of compiling one theme for one <see cref="DesignVariant" />: the identity, definition and
/// revision of the theme, the schema registry, the effective config, the global and language Token values by slot,
/// the global resources and the preset color palettes. Control Token values are materialized per control slot on
/// first use. A <c>ThemeContext</c> holds the snapshot that is current for its scope and replaces it when the theme
/// changes, so a captured snapshot keeps the values it was captured with.
/// AtomUI.Controls reads it in the Badge, Tag and preset color palette code to resolve colors.
/// </summary>
internal sealed class ThemeSnapshot
{
    internal ThemeSnapshot(
        string themeId,
        PreparedThemeDefinition definition,
        ThemeDefinitionRevision definitionRevision,
        ThemeContentFingerprint contentFingerprint,
        DesignVariant variant,
        ThemeSchemaRevision registryRevision,
        ThemeSchemaRegistry registry,
        NormalizedThemeConfig effectiveConfig,
        TokenValueTable globalTokenValues,
        TokenValueTable languageTokenValues,
        IReadOnlyDictionary<object, object?> globalResources,
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> presetColorPalettes,
        LazyControlSnapshots controlSlots)
    {
        ThemeId = themeId;
        Definition = definition;
        DefinitionRevision = definitionRevision;
        ContentFingerprint = contentFingerprint;
        Variant = variant;
        RegistryRevision = registryRevision;
        Registry = registry;
        EffectiveConfig = effectiveConfig;
        GlobalTokenValues = globalTokenValues;
        LanguageTokenValues = languageTokenValues;
        GlobalResources = globalResources;
        PresetColorPalettes = presetColorPalettes;
        Controls = controlSlots;
        // The global inputs and the slot table size are frozen at construction; the control part covers cold
        // source/config or materialized values.
        // A cell notifies the cache to recharge after it publishes for the first time, so the size taken at
        // insertion does not permanently ignore a later materialization.
        // Shared cells are conservatively charged to every snapshot that shares them; the estimate is still not
        // an exact byte count of the GC heap.
        _estimatedRetainedBytesBase = ThemeRetainedBytesEstimator.EstimateSnapshotBase(this);
    }

    internal string ThemeId { get; }
    internal PreparedThemeDefinition Definition { get; }
    internal ThemeDefinitionRevision DefinitionRevision { get; }
    internal ThemeContentFingerprint ContentFingerprint { get; }
    /// <summary>
    /// The variant the scope's global tokens were derived for.
    /// </summary>
    internal DesignVariant Variant { get; }

    internal ThemeAppearance Appearance => Variant.Appearance;

    /// <summary>
    /// The language this snapshot was derived by.
    /// </summary>
    internal Schema.DesignLanguageDescriptor Language => EffectiveConfig.Language!;
    internal ThemeSchemaRevision RegistryRevision { get; }
    internal ThemeSchemaRegistry Registry { get; }
    internal NormalizedThemeConfig EffectiveConfig { get; }
    internal TokenValueTable GlobalTokenValues { get; }

    /// <summary>
    /// The language token values, by language token slot.
    /// </summary>
    internal TokenValueTable LanguageTokenValues { get; }
    internal IReadOnlyDictionary<object, object?> GlobalResources { get; }
    internal IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> PresetColorPalettes { get; }
    internal LazyControlSnapshots Controls { get; }
    private readonly long _estimatedRetainedBytesBase;

    internal long EstimatedRetainedBytes =>
        ThemeRetainedBytesEstimator.Add(_estimatedRetainedBytesBase, Controls.EstimatedRetainedBytes);
}
