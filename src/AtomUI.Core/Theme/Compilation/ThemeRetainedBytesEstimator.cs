using System.Runtime.CompilerServices;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using Avalonia.Media;

namespace AtomUI.Theme.Compilation;

internal static class ThemeRetainedBytesEstimator
{
    // 编译器冻结的根资源和 palette 可跨快照共享，尺寸也按相同身份复用。
    private static readonly ConditionalWeakTable<IReadOnlyDictionary<object, object?>, RetainedBytes> s_rootResourceBytes = new();
    private static readonly ConditionalWeakTable<IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo>, RetainedBytes> s_paletteBytes = new();

    private const long ObjectOverhead = 32;
    private const long ReferenceBytes = 8;
    private const long DictionaryEntryBytes = 48;
    private const long ImmutableBrushBytes = 96;

    internal static long EstimateDenseValues(IReadOnlyList<object?> values)
    {
        var retainedBytes = Add(ObjectOverhead, values.Count * ReferenceBytes);
        foreach (var value in values)
        {
            retainedBytes = Add(retainedBytes, EstimateValue(value));
        }
        return retainedBytes;
    }

    internal static long EstimateControl(ControlThemeSnapshot control)
    {
        var retainedBytes = ObjectOverhead;
        retainedBytes = Add(retainedBytes, control.ControlTokenValues.EstimatedRetainedBytes);
        retainedBytes = Add(retainedBytes, EstimateSlotMap(control.EffectiveGlobalTokenDelta));
        retainedBytes = Add(retainedBytes, EstimateResourceMap(control.EffectiveGlobalResourceDelta));
        retainedBytes = Add(retainedBytes, EstimateResourceMap(control.ControlResources));
        return retainedBytes;
    }

    internal static long EstimateSnapshotBase(ThemeSnapshot snapshot)
    {
        var retainedBytes = Add(ObjectOverhead, EstimateValue(snapshot.ThemeId));
        retainedBytes = Add(retainedBytes, EstimateValue(snapshot.DefinitionRevision.SourceIdentity));
        retainedBytes = Add(retainedBytes, EstimateValue(snapshot.DefinitionRevision.SourceRevision));
        retainedBytes = Add(retainedBytes, EstimateValue(snapshot.DefinitionRevision.ContentDigest));
        retainedBytes = Add(retainedBytes, snapshot.GlobalTokenValues.EstimatedRetainedBytes);
        retainedBytes = Add(retainedBytes, EstimateRootResources(snapshot.GlobalResources));
        retainedBytes = Add(retainedBytes, EstimateRootPalettes(snapshot.PresetColorPalettes));
        retainedBytes = Add(retainedBytes, NormalizedThemeConfigCacheKey.Create(snapshot.EffectiveConfig).EstimatedInputRetainedBytes);
        retainedBytes = Add(retainedBytes, ThemeDefinitionCacheKey.Create(snapshot.Definition).EstimatedInputRetainedBytes);
        // 槽位表固定成本与冷 source/config 或已物化值分开计费；后者读取 cell 的发布状态，
        // 估算不会通过索引器触发控件物化。
        // 每槽独立 cell 与锁对象，按共享对象重复计费，避免低估多个冷槽的基础占用。
        retainedBytes = Add(retainedBytes, ObjectOverhead + snapshot.Controls.Count * (ReferenceBytes + 2 * ObjectOverhead + 4 * ReferenceBytes));
        return retainedBytes;
    }

    internal static long EstimateConfig(NormalizedThemeConfig config)
    {
        var bytes = 128L + config.Algorithms.Count * ReferenceBytes + config.Controls.Count * ReferenceBytes;
        bytes = Add(bytes, EstimateNormalizedTokens(config.GlobalTokens));
        foreach (var control in config.Controls)
        {
            bytes = Add(bytes, NormalizedControlConfigCacheKey.Create(control).EstimatedInputRetainedBytes);
        }
        return bytes;
    }

    internal static long EstimateConfig(NormalizedControlThemeConfig config)
    {
        var bytes = 128L + config.Algorithms.Count * ReferenceBytes;
        bytes = Add(bytes, EstimateValue(config.Identity.Catalog));
        bytes = Add(bytes, EstimateValue(config.Identity.Id));
        bytes = Add(bytes, EstimateNormalizedTokens(config.GlobalTokens));
        return Add(bytes, EstimateNormalizedTokens(config.OwnTokens));
    }

    private static long EstimateNormalizedTokens(IReadOnlyList<NormalizedTokenValue> tokens)
    {
        var bytes = 64L + tokens.Count * (ObjectOverhead + 4 * ReferenceBytes);
        foreach (var token in tokens)
        {
            bytes = Add(bytes, EstimateValue(token.Value));
            bytes = Add(bytes, EstimateValue(token.CanonicalValue));
        }
        return bytes;
    }

    internal static long EstimateDefinition(BoundThemeDefinition definition)
    {
        var bytes = 192L + definition.Algorithms.Count * ReferenceBytes + definition.Controls.Count * ReferenceBytes;
        bytes = Add(bytes, EstimateValue(definition.Id));
        bytes = Add(bytes, EstimateValue(definition.Name));
        bytes = Add(bytes, EstimateLocation(definition.Location));
        bytes = Add(bytes, EstimateBoundTokens(definition.Tokens));
        foreach (var control in definition.Controls)
        {
            bytes = Add(bytes, 160L + control.Algorithms.Count * ReferenceBytes);
            bytes = Add(bytes, EstimateLocation(control.Location));
            bytes = Add(bytes, EstimateBoundTokens(control.GlobalTokens));
            bytes = Add(bytes, EstimateBoundTokens(control.OwnTokens));
        }
        return bytes;
    }

    private static long EstimateBoundTokens(IReadOnlyList<BoundTokenValue> tokens)
    {
        var bytes = 64L + tokens.Count * (ObjectOverhead + 6 * ReferenceBytes);
        foreach (var token in tokens)
        {
            bytes = Add(bytes, EstimateValue(token.Value));
            bytes = Add(bytes, EstimateLocation(token.Location));
        }
        return bytes;
    }

    private static long EstimateLocation(ThemeSourceLocation location)
    {
        return Add(EstimateValue(location.Source), EstimateValue(location.Path));
    }

    internal static long EstimateLazySource(TokenValueTable values,
        IReadOnlyDictionary<object, object?> resources,
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> palettes,
        int algorithmCount,
        long keyBytes)
    {
        // source 可由不同代的 cell 共享；单个快照按不同 source 各计一次。
        // 预留恢复 builder 的值槽和 palette 成本，首次 Own 查询不会出现未计费的工作集。
        // descriptor、算法委托及其自定义闭包属于 registry 共享元数据，不递归遍历对象图。
        var bytes = Add(256L + algorithmCount * ReferenceBytes, keyBytes);
        bytes = Add(bytes, values.EstimatedRetainedBytes);
        bytes = Add(bytes, EstimateRootResources(resources));
        bytes = Add(bytes, EstimateRootPalettes(palettes));
        bytes = Add(bytes, values.EstimatedRetainedBytes);
        return Add(bytes, EstimateRootPalettes(palettes));
    }

    private static long EstimateSlotMap(IReadOnlyDictionary<int, object?> values)
    {
        var retainedBytes = Add(ObjectOverhead, values.Count * DictionaryEntryBytes);
        foreach (var value in values.Values)
        {
            retainedBytes = Add(retainedBytes, EstimateValue(value));
        }
        return retainedBytes;
    }

    private static long EstimateResourceMap(IReadOnlyDictionary<object, object?> values)
    {
        var retainedBytes = Add(ObjectOverhead, values.Count * DictionaryEntryBytes);
        foreach (var (key, value) in values)
        {
            retainedBytes = Add(retainedBytes, EstimateValue(key));
            retainedBytes = Add(retainedBytes, EstimateValue(value));
        }
        return retainedBytes;
    }

    private static long EstimateRootResources(IReadOnlyDictionary<object, object?> resources)
    {
        return s_rootResourceBytes.GetValue(resources, static value => new RetainedBytes(EstimateResourceMap(value))).Value;
    }

    private static long EstimateRootPalettes(IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> palettes)
    {
        return s_paletteBytes.GetValue(palettes, static value => new RetainedBytes(EstimatePalettes(value))).Value;
    }

    private static long EstimatePalettes(IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> palettes)
    {
        var retainedBytes = Add(ObjectOverhead, palettes.Count * DictionaryEntryBytes);
        foreach (var palette in palettes.Values)
        {
            retainedBytes = Add(retainedBytes, ObjectOverhead + 8 + palette.ColorSequence.Count * 4L);
        }
        return retainedBytes;
    }

    // 任意自定义对象只计固定壳体；不调用用户代码或反射遍历其私有保留图。
    internal static long EstimateValue(object? value)
    {
        return value switch
        {
            null => 0,
            string text => ObjectOverhead + text.Length * 2L,
            IBrush => ImmutableBrushBytes,
            FontFamily family => ObjectOverhead + EstimateValue(family.Name),
            ValueType => 24,
            _ => 64
        };
    }

    private sealed record RetainedBytes(long Value);

    // internal：ThemeSnapshot.EstimatedRetainedBytes 与 LazyControlSnapshots 的增量累计共用
    // 同一饱和语义（long.MaxValue 封顶），负值不允许进入 BoundedLruCache 尺寸记账。
    internal static long Add(long left, long right)
    {
        if (right < 0 || left > long.MaxValue - right)
        {
            return long.MaxValue;
        }
        return left + right;
    }
}
