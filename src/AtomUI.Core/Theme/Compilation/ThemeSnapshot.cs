using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

internal sealed class ThemeSnapshot
{
    internal ThemeSnapshot(
        string themeId,
        PreparedThemeDefinition definition,
        ThemeDefinitionRevision definitionRevision,
        ThemeContentFingerprint contentFingerprint,
        ThemeAppearance appearance,
        ThemeSchemaRevision registryRevision,
        ThemeSchemaRegistry registry,
        NormalizedThemeConfig effectiveConfig,
        TokenValueTable globalTokenValues,
        IReadOnlyDictionary<object, object?> globalResources,
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> presetColorPalettes,
        LazyControlSnapshots controlSlots)
    {
        ThemeId = themeId;
        Definition = definition;
        DefinitionRevision = definitionRevision;
        ContentFingerprint = contentFingerprint;
        Appearance = appearance;
        RegistryRevision = registryRevision;
        Registry = registry;
        EffectiveConfig = effectiveConfig;
        GlobalTokenValues = globalTokenValues;
        GlobalResources = globalResources;
        PresetColorPalettes = presetColorPalettes;
        Controls = controlSlots;
        // 全局输入和槽位表尺寸在构造期冻结；控件部分包含冷 source/config 或已物化值。
        // cell 首次发布后通知缓存重新计费，避免插入期尺寸永久忽略后续物化。
        // 共享 cell 保守重复计费；估算仍不是 GC 堆的精确字节统计。
        _estimatedRetainedBytesBase = ThemeRetainedBytesEstimator.EstimateSnapshotBase(this);
    }

    internal string ThemeId { get; }
    internal PreparedThemeDefinition Definition { get; }
    internal ThemeDefinitionRevision DefinitionRevision { get; }
    internal ThemeContentFingerprint ContentFingerprint { get; }
    internal ThemeAppearance Appearance { get; }
    internal ThemeSchemaRevision RegistryRevision { get; }
    internal ThemeSchemaRegistry Registry { get; }
    internal NormalizedThemeConfig EffectiveConfig { get; }
    internal TokenValueTable GlobalTokenValues { get; }
    internal IReadOnlyDictionary<object, object?> GlobalResources { get; }
    internal IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> PresetColorPalettes { get; }
    internal LazyControlSnapshots Controls { get; }
    private readonly long _estimatedRetainedBytesBase;

    internal long EstimatedRetainedBytes =>
        ThemeRetainedBytesEstimator.Add(_estimatedRetainedBytesBase, Controls.EstimatedRetainedBytes);
}
