using System.Runtime.CompilerServices;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using Avalonia.Media;

namespace AtomUI.Theme.Compilation;

internal static class ThemeRetainedBytesEstimator
{
    // Root resources and palettes frozen by the compiler may be shared by several snapshots, so their size
    // is reused for the same instance.
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

    internal static long EstimateSnapshotProductionInput(ThemeCompileInput input, ThemeSnapshotCacheKey key)
    {
        // Keys retain normalized identity data; the factory additionally captures the original
        // definition/configuration and a reusable parent, including its cold sources and cells.
        // Invoke only on new production admission: parent size can walk all its controls.
        var bytes = Add(128, key.EstimatedInputRetainedBytes);
        if (input.ReusableParent is { } parent)
        {
            bytes = Add(bytes, parent.EstimatedRetainedBytes);
        }
        if (input.ChangesFromReusableParent is { } changes)
        {
            bytes = Add(bytes, 64L + changes.ChangedControls.Count * 32L);
            foreach (var identity in changes.ChangedControls)
            {
                bytes = Add(bytes, CacheKeyArray.EstimateIdentity(identity));
            }
        }
        // Registry descriptors and arbitrary user callback graphs remain shared metadata;
        // do not invoke or reflect over user objects to estimate a production closure.
        return bytes;
    }

    internal static long EstimateSnapshotBase(ThemeSnapshot snapshot)
    {
        var retainedBytes = Add(ObjectOverhead, EstimateValue(snapshot.ThemeId));
        retainedBytes = Add(retainedBytes, EstimateValue(snapshot.DefinitionRevision.SourceIdentity));
        retainedBytes = Add(retainedBytes, EstimateValue(snapshot.DefinitionRevision.SourceRevision));
        retainedBytes = Add(retainedBytes, EstimateValue(snapshot.DefinitionRevision.ContentDigest));
        retainedBytes = Add(retainedBytes, EstimateValue(snapshot.DefinitionRevision.ProgramDigest));
        retainedBytes = Add(retainedBytes, snapshot.GlobalTokenValues.EstimatedRetainedBytes);
        retainedBytes = Add(retainedBytes, EstimateRootResources(snapshot.GlobalResources));
        retainedBytes = Add(retainedBytes, EstimateRootPalettes(snapshot.PresetColorPalettes));
        retainedBytes = Add(retainedBytes, NormalizedThemeConfigCacheKey.Create(snapshot.EffectiveConfig).EstimatedInputRetainedBytes);
        retainedBytes = Add(retainedBytes, ThemeDefinitionCacheKey.Create(snapshot.Definition).EstimatedInputRetainedBytes);
        // The fixed cost of the slot table is charged separately from cold source/config or materialized values.
        // The latter read the cell's published state, so the estimate never materializes a control through the
        // indexer.
        // Each slot has its own cell and lock object; like shared objects they are charged once per slot, so the
        // base footprint of many cold slots is not underestimated.
        retainedBytes = Add(retainedBytes, ObjectOverhead + snapshot.Controls.Count * (ReferenceBytes + 2 * ObjectOverhead + 4 * ReferenceBytes));
        return retainedBytes;
    }

    internal static long EstimateConfig(NormalizedThemeConfig config) =>
        EstimateConfig(config, new HashSet<NormalizedThemeConfig>(ReferenceEqualityComparer.Instance));

    private static long EstimateConfig(NormalizedThemeConfig config, HashSet<NormalizedThemeConfig> visited)
    {
        if (!visited.Add(config))
        {
            return 0;
        }
        var bytes = 160L + config.ControlIdentities.Count * ReferenceBytes;
        bytes = Add(bytes, EstimateNormalizedTokens(config.GlobalTokens));
        foreach (var token in config.LanguageTokens)
        {
            bytes = Add(bytes, ObjectOverhead + 2 * ReferenceBytes);
            bytes = Add(bytes, EstimateValue(token.Name));
            bytes = Add(bytes, EstimateValue(token.Value));
        }
        foreach (var control in config.Controls)
        {
            bytes = Add(bytes, NormalizedControlConfigCacheKey.Create(control).EstimatedInputRetainedBytes);
        }
        return bytes;
    }

    internal static long EstimateConfig(NormalizedControlThemeConfig config)
    {
        var bytes = 128L;
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

    internal static long EstimateDefinition(PreparedThemeDefinition definition)
    {
        var bytes = 192L + definition.ControlIdentities.Count * ReferenceBytes;
        bytes = Add(bytes, EstimateValue(definition.Id));
        bytes = Add(bytes, EstimateValue(definition.Name));
        bytes = Add(bytes, EstimateLocation(definition.Location));
        bytes = Add(bytes, EstimateSection(definition.Base));
        foreach (var condition in definition.Conditions)
        {
            bytes = Add(bytes, 96L);
            bytes = Add(bytes, EstimateSection(condition.Values));
        }
        return bytes;
    }

    private static long EstimateSection(ThemeSectionDefinition section)
    {
        var bytes = Add(64L, EstimateBoundTokens(section.Tokens));
        bytes = Add(bytes, EstimateBoundTokens(section.LanguageTokens));
        foreach (var control in section.Controls)
        {
            bytes = Add(bytes, 160L);
            bytes = Add(bytes, EstimateLocation(control.Location));
            bytes = Add(bytes, EstimateBoundTokens(control.GlobalTokens));
            bytes = Add(bytes, EstimateBoundTokens(control.OwnTokens));
        }
        return bytes;
    }

    private static long EstimateDeferredControls(IReadOnlyList<Schema.ControlTokenIdentity> identities, string revision)
    {
        // Count the immutable declaration index and its deferred factories. Estimation must not
        // invoke factories; compiled control snapshots are budgeted by their own cache on first use.
        var bytes = EstimateValue(revision);
        foreach (var identity in identities)
        {
            bytes = Add(bytes, 256L + EstimateValue(identity.Catalog) + EstimateValue(identity.Id));
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
        // A source may be shared by cells of different generations; a snapshot charges each distinct source once.
        // The value slots and palettes of the builder restored on first use are reserved here, so the first
        // Own-token query does not bring in an uncharged working set.
        // Descriptors, algorithm delegates and their custom closures are metadata shared through the registry;
        // their object graphs are not walked.
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

    // Any other object is charged a fixed shell only; estimation neither calls user code nor reflects over
    // the object's private retained graph.
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

    // Internal: ThemeSnapshot.EstimatedRetainedBytes and LazyControlSnapshots accumulate increments with the
    // same saturating semantics (capped at long.MaxValue); a negative addend saturates too, so no negative
    // size enters the BoundedLruCache accounting.
    internal static long Add(long left, long right)
    {
        if (right < 0 || left > long.MaxValue - right)
        {
            return long.MaxValue;
        }
        return left + right;
    }
}
