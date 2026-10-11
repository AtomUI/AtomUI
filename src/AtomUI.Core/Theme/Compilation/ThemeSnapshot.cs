using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

internal sealed class ThemeSnapshot
{
    internal ThemeSnapshot(
        string themeId,
        BoundThemeDefinition definition,
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
        // 全局部分 + 槽位表开销在构造期冻结；控件快照字节随首访物化增量累计。
        // 已知取舍（设计文档 §4）：ThemeSnapshotCache / ControlCompilationCache 在插入时采样本值，
        // 物化增长不回灌——估算是启发式容量阀，漂移上界 = 实消费槽位字节（≤187 槽位 × 单槽 KB 级），
        // 驱逐只影响性能不影响正确性。
        _estimatedRetainedBytesBase = ThemeRetainedBytesEstimator.EstimateSnapshotBase(this);
    }

    internal string ThemeId { get; }
    internal BoundThemeDefinition Definition { get; }
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
        ThemeRetainedBytesEstimator.Add(_estimatedRetainedBytesBase, Controls.MaterializedBytes);
}
