using System.Buffers;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using AtomUI.Generated.AtomUICore;
using Avalonia.Media;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;

namespace AtomUI.Theme.Compilation;

internal sealed class ThemeCompiler
{
    private static readonly ConditionalWeakTable<IReadOnlyList<TokenDescriptor>, CanonicalGlobalSchema> GlobalSchemas = new();

    private const string CompilerPath = "ThemeCompiler";
    private static readonly ConditionalWeakTable<
        PreparedThemeDefinition,
        ConditionalWeakTable<ThemeSchemaRegistry, NormalizedThemeConfig>> s_definitionDefaults = new();
    private static readonly ConditionalWeakTable<
        ThemeSchemaRegistry,
        NormalizedThemeConfig> s_libraryDefaults = new();

    internal static NormalizedThemeConfig CreateDefinitionDefaults(
        PreparedThemeDefinition definition,
        ThemeSchemaRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(registry);

        var byRegistry = s_definitionDefaults.GetValue(
            definition,
            static _ => new ConditionalWeakTable<ThemeSchemaRegistry, NormalizedThemeConfig>());
        return byRegistry.GetValue(
            registry,
            currentRegistry => CreateDefinitionDefaultsCore(definition, currentRegistry));
    }

    private static NormalizedThemeConfig CreateDefinitionDefaultsCore(
        PreparedThemeDefinition definition,
        ThemeSchemaRegistry registry)
    {
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms = definition.Algorithms;
        if (algorithms.Count == 0)
        {
            registry.TryGetAlgorithm(ThemeAlgorithm.Default, out var defaultAlgorithm);
            algorithms =
            [
                defaultAlgorithm ??
                throw new InvalidOperationException("Default algorithm is not registered.")
            ];
        }

        var globalTokens = new NormalizedTokenValue[definition.Tokens.Count];
        for (var index = 0; index < globalTokens.Length; index++)
        {
            var token = definition.Tokens[index];
            globalTokens[index] = new NormalizedTokenValue(
                token.Descriptor,
                token.Value,
                token.CanonicalValue);
        }
        Array.Sort(
            globalTokens,
            static (left, right) => left.Descriptor.Slot.CompareTo(right.Descriptor.Slot));

        NormalizedControlThemeConfig ResolveControl(ControlTokenIdentity identity)
        {
            var control = definition.GetControl(identity);
            var mode = control.AlgorithmMode == ControlAlgorithmMode.Unspecified
                ? ControlAlgorithmMode.Disabled : control.AlgorithmMode;
            return NormalizedControlThemeConfig.CreateCanonical(control.Descriptor.Identity, mode,
                mode == ControlAlgorithmMode.Custom ? ThemeConfigArray.Copy(control.Algorithms) : Array.Empty<ThemeAlgorithmDescriptor>(),
                NormalizeBoundTokens(control.GlobalTokens), NormalizeBoundTokens(control.OwnTokens));
        }
        if (definition.ControlRevision is { } revision)
        {
            return NormalizedThemeConfig.CreateDeferred(false, true, ThemeConfigArray.Copy(algorithms), globalTokens,
                definition.ControlIdentities, identity => ResolveControl(identity), revision, (definition, registry));
        }
        var controls = definition.ControlIdentities.Select(ResolveControl).ToArray();
        Array.Sort(controls, static (left, right) => CompareIdentity(left.Identity, right.Identity));
        return NormalizedThemeConfig.CreateCanonical(false, true, ThemeConfigArray.Copy(algorithms), globalTokens, controls);
    }

    internal static NormalizedThemeConfig CreateLibraryDefaults(
        ThemeSchemaRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        return s_libraryDefaults.GetValue(registry, CreateLibraryDefaultsCore);
    }

    private static NormalizedThemeConfig CreateLibraryDefaultsCore(ThemeSchemaRegistry registry)
    {
        if (!registry.TryGetAlgorithm(ThemeAlgorithm.Default, out var defaultAlgorithm))
        {
            throw new InvalidOperationException("Default algorithm is not registered.");
        }

        return NormalizedThemeConfig.CreateCanonical(
            false,
            true,
            [defaultAlgorithm],
            Array.Empty<NormalizedTokenValue>(),
            Array.Empty<NormalizedControlThemeConfig>());
    }

    private static NormalizedTokenValue[] NormalizeBoundTokens(
        IReadOnlyList<BoundTokenValue> tokens)
    {
        var result = new NormalizedTokenValue[tokens.Count];
        for (var index = 0; index < result.Length; index++)
        {
            var token = tokens[index];
            result[index] = new NormalizedTokenValue(
                token.Descriptor,
                token.Value,
                token.CanonicalValue);
        }
        Array.Sort(
            result,
            static (left, right) => left.Descriptor.Slot.CompareTo(right.Descriptor.Slot));
        return result;
    }

    internal ThemeCompileResult Compile(ThemeCompileInput input)
    {
        return Compile(input, null);
    }

    internal ThemeCompileResult Compile(
        ThemeCompileInput input,
        ControlCompilationCache? controlCompilationCache)
        => CompileCore(input, controlCompilationCache, preserveAppendedSlots: false);

    // Only the package transaction calls this: it preserves committed definitions/configuration,
    // and registry entry identity proves that callbacks and slot assignments did not change.
    internal ThemeCompileResult CompilePackageAppend(ThemeCompileInput input) =>
        CompileCore(input, null, preserveAppendedSlots: true);

    private ThemeCompileResult CompileCore(ThemeCompileInput input,
        ControlCompilationCache? controlCompilationCache, bool preserveAppendedSlots)
    {
        ArgumentNullException.ThrowIfNull(input);

        var diagnostics = new List<ThemeDefinitionDiagnostic>();
        try
        {
            if (input.EffectiveConfig.Algorithms.Count == 0)
            {
                AddError(diagnostics, "ATMTHM5001", "A theme compilation requires at least one algorithm.");
                return Failed(diagnostics);
            }

            if (preserveAppendedSlots && (input.ReusableParent is not { } previousInput ||
                previousInput.DefinitionRevision != input.DefinitionRevision ||
                !CanReuseGlobal(input, ResolveAppearance(input.EffectiveConfig.Algorithms))))
            {
                throw new InvalidOperationException("Package append cannot change committed global theme inputs.");
            }
            var global = CompileGlobal(input);
            var controls = CompileControls(input, global, controlCompilationCache);
            if (preserveAppendedSlots)
            {
                var previous = input.ReusableParent!;
                if (!global.ReuseGlobal || previous.DefinitionRevision != input.DefinitionRevision)
                {
                    throw new InvalidOperationException("Package append cannot change committed global theme inputs.");
                }
                var cells = Enumerable.Range(0, controls.Count).Select(controls.GetCell).ToArray();
                for (var slot = 0; slot < previous.Controls.Count; slot++)
                {
                    if (!input.Registry.HasSameControlSlot(previous.Registry, slot))
                    {
                        throw new InvalidOperationException("Package append cannot replace an existing Control slot or callback.");
                    }
                    var previousCell = previous.Controls.GetCell(slot);
                    // A materialized cell contains only its value. A cold cell still owns
                    // lazy factories from the previous registry/index generation; use the
                    // freshly constructed cell for the new generation instead.
                    if (previousCell.MaterializedValue is not null)
                    {
                        cells[slot] = previousCell;
                    }
                }
                controls = new LazyControlSnapshots(cells);
            }
            var fingerprint = ThemeContentFingerprint.Compute(
                input.DefinitionRevision,
                input.EffectiveConfig,
                input.Registry.Revision,
                global.Appearance);

            return new ThemeCompileResult(
                new ThemeSnapshot(
                    input.Definition.Id,
                    input.Definition,
                    input.DefinitionRevision,
                    fingerprint,
                    global.Appearance,
                    input.Registry.Revision,
                    input.Registry,
                    input.EffectiveConfig,
                    global.Values,
                    global.Resources,
                    global.Palettes,
                    controls),
                CopyDiagnostics(diagnostics),
                null);
        }
        catch (Exception exception)
        {
            return new ThemeCompileResult(null, CopyDiagnostics(diagnostics), exception);
        }
    }

    private readonly record struct GlobalCompileOutcome(
        ThemeAppearance Appearance,
        bool ReuseGlobal,
        DesignToken? Builder,
        TokenValueTable Values,
        IReadOnlyDictionary<object, object?> Resources,
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> Palettes);

    private static GlobalCompileOutcome CompileGlobal(ThemeCompileInput input)
    {
        var appearance = ResolveAppearance(input.EffectiveConfig.Algorithms);
        var reuseGlobal = CanReuseGlobal(input, appearance);
        DesignToken? globalBuilder;
        TokenValueTable globalValues;
        IReadOnlyDictionary<object, object?> globalResources;
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> palettes;
        if (reuseGlobal)
        {
            var parent = input.ReusableParent!;
            globalValues    = parent.GlobalTokenValues;
            globalResources = parent.GlobalResources;
            palettes        = parent.PresetColorPalettes;
            globalBuilder   = null;
        }
        else
        {
            globalBuilder   = CompileGlobalBuilder(input);
            globalValues    = TokenValueTable.Freeze(globalBuilder, input.Registry.GlobalTokens);
            globalResources = BuildResourceMap(globalBuilder, input.Registry.GlobalTokens, globalValues);
            palettes        = FreezePresetPalettes(globalBuilder);
        }

        return new GlobalCompileOutcome(
            appearance,
            reuseGlobal,
            globalBuilder,
            globalValues,
            globalResources,
            palettes);
    }

    private static bool CanReuseGlobal(ThemeCompileInput input, ThemeAppearance appearance)
    {
        var parent = input.ReusableParent;
        if (parent is null ||
            !input.Registry.HasSameGlobalSchema(parent.Registry) ||
            parent.Appearance != appearance)
        {
            return false;
        }

        if (input.ChangesFromReusableParent is { } changes)
        {
            return !changes.AlgorithmsChanged && !changes.GlobalTokensChanged;
        }

        return AlgorithmSequenceEqual(
                   parent.EffectiveConfig.Algorithms,
                   input.EffectiveConfig.Algorithms) &&
               TokenSequenceEqual(
                   parent.EffectiveConfig.GlobalTokens,
                   input.EffectiveConfig.GlobalTokens);
    }

    // 先按有序游标解析配置和复用状态，仅在首次查询时计算控件值与差量。
    private static LazyControlSnapshots CompileControls(
        ThemeCompileInput input,
        GlobalCompileOutcome global,
        ControlCompilationCache? controlCompilationCache)
    {
        var count = input.Registry.Controls.Count;
        if (input.Registry.UsesStartupIndex || input.EffectiveConfig.HasDeferredControls ||
            input.ReusableParent?.EffectiveConfig.HasDeferredControls == true)
        {
            return CompileIndexedControls(input, global, controlCompilationCache, count);
        }
        var cells = new ControlSnapshotCell[count];
        var configIndex = 0;
        var parentConfigIndex = 0;
        var changedControlIndex = 0;
        var effectiveControls = input.EffectiveConfig.Controls;
        var previousControls = input.ReusableParent?.EffectiveConfig.Controls;
        LazyControlSnapshotSource? source = null;
        for (var slot = 0; slot < count; slot++)
        {
            var descriptor = input.Registry.Controls[slot];
            var config = FindControlConfig(
                effectiveControls,
                descriptor.Identity,
                ref configIndex);
            var parentConfig = input.ReusableParent is null
                ? null
                : FindControlConfig(
                    previousControls!,
                    descriptor.Identity,
                    ref parentConfigIndex);
            bool? controlChanged = input.ChangesFromReusableParent is null
                ? null
                : IsControlChanged(
                    input.ChangesFromReusableParent.ChangedControls,
                    descriptor.Identity,
                    ref changedControlIndex);
            if (CanReuseControl(
                    input.ReusableParent,
                    input.Registry,
                    descriptor,
                    config,
                    parentConfig,
                    global.ReuseGlobal,
                    global.Appearance,
                    controlChanged))
            {
                cells[slot] = input.ReusableParent!.Controls.GetCell(slot);
                continue;
            }
            if (config is null && !descriptor.HasOwnTokens)
            {
                cells[slot] = new ControlSnapshotCell(new ControlThemeSnapshot(
                    descriptor.Slot,
                    global.Appearance,
                    FrozenDictionary<int, object?>.Empty,
                    FrozenDictionary<object, object?>.Empty,
                    TokenValueTable.Empty,
                    FrozenDictionary<object, object?>.Empty));
                continue;
            }

            source ??= new LazyControlSnapshotSource(
                input,
                global.Appearance,
                global.Builder,
                global.Values,
                global.Resources,
                global.Palettes,
                controlCompilationCache);
            cells[slot] = new ControlSnapshotCell(source, descriptor, config);
        }
        return new LazyControlSnapshots(cells);
    }

    private static LazyControlSnapshots CompileIndexedControls(ThemeCompileInput input, GlobalCompileOutcome global,
        ControlCompilationCache? cache, int count)
    {
        var cells = new ControlSnapshotCell[count];
        var configured = new HashSet<ControlTokenIdentity>(input.EffectiveConfig.ControlIdentities);
        var effectiveConfig = input.EffectiveConfig;
        var parent = input.ReusableParent;
        var canReuse = parent is not null && ReferenceEquals(parent.Registry, input.Registry) &&
            parent.Appearance == global.Appearance && global.ReuseGlobal;
        var allControlsUnchanged = canReuse && effectiveConfig.Equals(parent!.EffectiveConfig);
        var changedControls = canReuse && input.ChangesFromReusableParent is { } changes
            ? changes.ChangedControls.ToHashSet()
            : null;
        LazyControlSnapshotSource? source = null;
        for (var slot = 0; slot < count; slot++)
        {
            var identity = input.Registry.GetControlIdentity(slot);
            if (allControlsUnchanged || (canReuse && changedControls is not null && !changedControls.Contains(identity)))
            {
                cells[slot] = parent!.Controls.GetCell(slot);
                continue;
            }
            var hasConfig = configured.Contains(identity);
            if (!hasConfig && !input.Registry.ControlHasOwnTokens(slot))
            {
                cells[slot] = new ControlSnapshotCell(new ControlThemeSnapshot(slot, global.Appearance,
                    FrozenDictionary<int, object?>.Empty, FrozenDictionary<object, object?>.Empty,
                    TokenValueTable.Empty, FrozenDictionary<object, object?>.Empty));
                continue;
            }
            source ??= new LazyControlSnapshotSource(input, global.Appearance, global.Builder, global.Values,
                global.Resources, global.Palettes, cache);
            ControlThemeSnapshot? previousValue = null;
            NormalizedControlThemeConfig? previousConfig = null;
            if (canReuse && parent!.Controls.GetCell(slot).MaterializedValue is { } materialized)
            {
                previousValue = materialized;
                // This control was already read in the parent; its deferred config is cached.
                // Keep only the per-control value, not the previous full effective config.
                parent.EffectiveConfig.TryGetControl(identity, out previousConfig);
            }
            var controlSlot = slot;
            var registry = input.Registry;
            cells[slot] = new ControlSnapshotCell(source, () => registry.Controls[controlSlot],
                () => effectiveConfig.TryGetControl(identity, out var config) ? config : null, !hasConfig,
                previousValue, previousConfig);
        }
        return new LazyControlSnapshots(cells);
    }

    private static bool CanReuseControl(
        ThemeSnapshot? parent,
        ThemeSchemaRegistry registry,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config,
        NormalizedControlThemeConfig? parentConfig,
        bool reuseGlobal,
        ThemeAppearance appearance,
        bool? controlChanged)
    {
        // slot 仅在当前 schema 内有效；演化重排后不能按旧数组下标复用其他控件的差量。
        if (parent is null || parent.RegistryRevision != registry.Revision)
        {
            return false;
        }
        if ((uint)descriptor.Slot >= (uint)parent.Controls.Count)
        {
            return false;
        }
        var previousDescriptor = parent.Registry.Controls[descriptor.Slot];
        if (previousDescriptor.Identity != descriptor.Identity ||
            previousDescriptor.ControlType != descriptor.ControlType ||
            !ReferenceEquals(previousDescriptor.CompilationIdentity, descriptor.CompilationIdentity))
        {
            return false;
        }

        if (controlChanged == true)
        {
            return false;
        }
        if (config is null && parentConfig is null && !descriptor.HasOwnTokens)
        {
            return parent.Appearance == appearance;
        }

        if (!reuseGlobal)
        {
            return false;
        }

        return controlChanged == false ||
               (config is null ? parentConfig is null : config.Equals(parentConfig));
    }

    // 由独立 lazy source 在首次查询时执行，不依赖历史快照。
    internal static ControlThemeSnapshot CompileControl(
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config,
        IReadOnlyList<ThemeAlgorithmDescriptor> globalAlgorithms,
        ThemeAppearance globalAppearance,
        DesignToken globalBuilder,
        TokenValueTable globalValues,
        IReadOnlyDictionary<object, object?> globalResources,
        IReadOnlyList<TokenDescriptor> globalDescriptors)
    {
        var effectiveGlobalBuilder = CopyBuilder(globalBuilder, globalDescriptors);
        var appearance = globalAppearance;
        var effectiveGlobalMatchesRoot = config is null;
        if (config is not null)
        {
            switch (config.AlgorithmMode)
            {
                case ControlAlgorithmMode.Global:
                    effectiveGlobalBuilder = ReevaluateControlGlobal(
                        effectiveGlobalBuilder,
                        config.GlobalTokens,
                        globalAlgorithms,
                        globalDescriptors);
                    appearance = ResolveAppearance(globalAppearance, globalAlgorithms);
                    break;

                case ControlAlgorithmMode.Custom:
                    effectiveGlobalBuilder = ReevaluateControlGlobal(
                        effectiveGlobalBuilder,
                        config.GlobalTokens,
                        config.Algorithms,
                        globalDescriptors);
                    appearance = ResolveAppearance(globalAppearance, config.Algorithms);
                    break;

                case ControlAlgorithmMode.Unspecified:
                case ControlAlgorithmMode.Disabled:
                    ApplyAllValues(effectiveGlobalBuilder, config.GlobalTokens);
                    NormalizeFinalBehavior(effectiveGlobalBuilder);
                    effectiveGlobalMatchesRoot = config.GlobalTokens.Count == 0;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Control '{descriptor.Identity}' has unsupported algorithm mode '{config.AlgorithmMode}'.");
            }
        }

        IReadOnlyDictionary<int, object?> globalDelta;
        IReadOnlyDictionary<object, object?> globalResourceDelta;
        if (effectiveGlobalMatchesRoot)
        {
            globalDelta = FrozenDictionary<int, object?>.Empty;
            globalResourceDelta = FrozenDictionary<object, object?>.Empty;
        }
        else
        {
            var effectiveGlobalValues = TokenValueTable.Freeze(effectiveGlobalBuilder, globalDescriptors);
            var effectiveGlobalResources = BuildResourceMap(effectiveGlobalBuilder, globalDescriptors, effectiveGlobalValues);
            globalDelta = BuildTokenDelta(globalValues, effectiveGlobalValues);
            globalResourceDelta = BuildDenseResourceDelta(globalResources, effectiveGlobalResources);
        }

        var controlBuilder = descriptor.CreateBuilder();
        var controlTokenValues = TokenValueTable.Empty;
        IReadOnlyDictionary<object, object?> controlResources =
            FrozenDictionary<object, object?>.Empty;
        if (controlBuilder is not null)
        {
            controlBuilder.AssignEffectiveGlobalToken(effectiveGlobalBuilder);
            if (descriptor.EvaluationVersion == ControlTokenEvaluationVersion.DependencyDefaultsV2)
            {
                if (config is not null)
                {
                    ApplyAllValues(controlBuilder, config.OwnTokens);
                }
                EvaluateControlDefaults(descriptor, controlBuilder, effectiveGlobalBuilder, appearance,
                    config?.OwnTokens ?? Array.Empty<NormalizedTokenValue>());
            }
            else
            {
                descriptor.Evaluate(controlBuilder, appearance);
                if (config is not null)
                {
                    ApplyAllValues(controlBuilder, config.OwnTokens);
                }
            }

            descriptor.ApplyAliases(controlBuilder);
            controlTokenValues = TokenValueTable.Freeze(controlBuilder, descriptor.OwnTokens);
            controlResources = BuildResourceMap(controlBuilder, descriptor.OwnTokens);
        }

        return new ControlThemeSnapshot(
            descriptor.Slot,
            appearance,
            globalDelta,
            globalResourceDelta,
            controlTokenValues,
            controlResources);
    }

    private static void EvaluateControlDefaults(ControlTokenDescriptor descriptor,
        AbstractControlDesignToken builder, DesignToken effectiveGlobal, ThemeAppearance appearance,
        IReadOnlyList<NormalizedTokenValue> explicitValues)
    {
        var context = new ControlTokenDefaultContext(effectiveGlobal, appearance);
        if (explicitValues.Count == 0)
        {
            descriptor.EvaluateDefaults(builder, in context, []);
            return;
        }

        var wordCount = (int)((descriptor.OwnTokens.Count + 63L) >> 6);
        ulong[]? rented = null;
        Span<ulong> presence = wordCount <= 64
            ? stackalloc ulong[wordCount]
            : (rented = ArrayPool<ulong>.Shared.Rent(wordCount)).AsSpan(0, wordCount);
        try
        {
            presence.Clear();
            foreach (var value in explicitValues)
            {
                var slot = value.Descriptor.Slot;
                presence[slot >> 6] |= 1UL << (slot & 63);
            }
            descriptor.EvaluateDefaults(builder, in context, presence);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<ulong>.Shared.Return(rented, clearArray: true);
            }
        }
    }

    private static DesignToken ReevaluateControlGlobal(
        DesignToken baseline,
        IReadOnlyList<NormalizedTokenValue> overrides,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms,
        IReadOnlyList<TokenDescriptor> descriptors)
    {
        ApplyValues(baseline, overrides, TokenStage.Seed);
        var result = EvaluateAlgorithms(baseline, algorithms, descriptors);
        ApplyValues(result, overrides, TokenStage.Map);
        result.CalculateAliasTokenValues();
        ApplyValues(result, overrides, TokenStage.Alias);
        NormalizeFinalBehavior(result);
        return result;
    }

    private static ThemeAppearance ResolveAppearance(
        ThemeAppearance baseline,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms)
    {
        var appearance = baseline;
        foreach (var algorithm in algorithms)
        {
            appearance = algorithm.AppearanceEffect switch
            {
                ThemeAppearanceEffect.Light => ThemeAppearance.Light,
                ThemeAppearanceEffect.Dark  => ThemeAppearance.Dark,
                _                           => appearance
            };
        }

        return appearance;
    }

    private static NormalizedControlThemeConfig? FindControlConfig(
        IReadOnlyList<NormalizedControlThemeConfig> controls,
        ControlTokenIdentity identity,
        ref int index)
    {
        while (index < controls.Count)
        {
            var control = controls[index];
            var comparison = CompareIdentity(control.Identity, identity);
            if (comparison < 0)
            {
                index++;
                continue;
            }
            if (comparison == 0)
            {
                index++;
                return control;
            }
            break;
        }

        return null;
    }

    private static bool IsControlChanged(
        IReadOnlyList<ControlTokenIdentity> changedControls,
        ControlTokenIdentity identity,
        ref int index)
    {
        while (index < changedControls.Count)
        {
            var comparison = CompareIdentity(changedControls[index], identity);
            if (comparison < 0)
            {
                index++;
                continue;
            }
            if (comparison == 0)
            {
                index++;
                return true;
            }
            break;
        }
        return false;
    }

    private static int CompareIdentity(ControlTokenIdentity left, ControlTokenIdentity right)
    {
        var catalog = string.Compare(left.Catalog, right.Catalog, StringComparison.Ordinal);
        return catalog != 0 ? catalog : string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static IReadOnlyDictionary<int, object?> BuildTokenDelta(
        TokenValueTable globalValues,
        TokenValueTable effectiveValues)
    {
        var delta = new Dictionary<int, object?>();
        for (var slot = 0; slot < effectiveValues.Count; slot++)
        {
            var value = effectiveValues.GetValue(slot);
            if (!globalValues.ValueEquals(slot, value))
            {
                delta.Add(slot, value);
            }
        }

        return delta.ToFrozenDictionary();
    }

    private static IReadOnlyDictionary<object, object?> BuildDenseResourceDelta(
        IReadOnlyDictionary<object, object?> globalResources,
        IReadOnlyDictionary<object, object?> effectiveResources)
    {
        var delta = new Dictionary<object, object?>();
        foreach (var resource in effectiveResources)
        {
            if (!globalResources.TryGetValue(resource.Key, out var globalValue) ||
                !Equals(globalValue, resource.Value))
            {
                delta.Add(resource.Key, resource.Value);
            }
        }

        return delta.ToFrozenDictionary();
    }

    private static bool AlgorithmSequenceEqual(
        IReadOnlyList<ThemeAlgorithmDescriptor> left,
        IReadOnlyList<ThemeAlgorithmDescriptor> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Count; index++)
        {
            if (!ReferenceEquals(left[index].CompilationIdentity, right[index].CompilationIdentity))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TokenSequenceEqual(
        IReadOnlyList<NormalizedTokenValue> left,
        IReadOnlyList<NormalizedTokenValue> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Count; index++)
        {
            if (!left[index].Equals(right[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static ThemeAppearance ResolveAppearance(
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms)
    {
        var appearance = ThemeAppearance.Light;
        foreach (var algorithm in algorithms)
        {
            appearance = algorithm.AppearanceEffect switch
            {
                ThemeAppearanceEffect.Light => ThemeAppearance.Light,
                ThemeAppearanceEffect.Dark  => ThemeAppearance.Dark,
                _                           => appearance
            };
        }

        return appearance;
    }

    private static DesignToken CompileGlobalBuilder(ThemeCompileInput input)
    {
        var seed = new DesignToken();
        ApplyValues(seed, input.EffectiveConfig.GlobalTokens, TokenStage.Seed);
        var map = EvaluateAlgorithms(seed, input.EffectiveConfig.Algorithms, input.Registry.GlobalTokens);
        ApplyValues(map, input.EffectiveConfig.GlobalTokens, TokenStage.Map);
        map.CalculateAliasTokenValues();
        ApplyValues(map, input.EffectiveConfig.GlobalTokens, TokenStage.Alias);
        NormalizeFinalBehavior(map);
        return map;
    }

    private static DesignToken EvaluateAlgorithms(
        DesignToken effectiveSeed,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms,
        IReadOnlyList<TokenDescriptor> descriptors)
    {
        DesignToken? previousMap = null;
        foreach (var algorithm in algorithms)
        {
            var nextMap = CopyBuilder(previousMap ?? effectiveSeed, descriptors);
            algorithm.Create().Evaluate(effectiveSeed, previousMap, nextMap);
            previousMap = nextMap;
        }

        return previousMap ?? throw new InvalidOperationException(
            "A theme compilation requires at least one algorithm.");
    }

    private sealed class CanonicalGlobalSchema
    {
        internal CanonicalGlobalSchema(IReadOnlyList<TokenDescriptor> descriptors)
        {
            var canonical = GeneratedThemeSchema.GetGlobalTokens();
            IsCanonical = descriptors.Count == canonical.Count;
            for (var index = 0; IsCanonical && index < canonical.Count; index++)
            {
                IsCanonical = ReferenceEquals(descriptors[index], canonical[index]);
            }
        }

        internal bool IsCanonical { get; }
    }

    private static bool IsCanonicalGlobalSchema(IReadOnlyList<TokenDescriptor> descriptors)
    {
        // Compiler inputs are registry-owned immutable lists. Metadata equality cannot certify callbacks.
        return GlobalSchemas.GetValue(descriptors, static value => new CanonicalGlobalSchema(value)).IsCanonical;
    }

    private static DesignToken CopyBuilder(
        DesignToken source,
        IReadOnlyList<TokenDescriptor> descriptors)
    {
        var canonical = IsCanonicalGlobalSchema(descriptors);
        var copy = new DesignToken(initializeSeed: !canonical);
        if (canonical)
        {
            GeneratedThemeSchema.CopyGlobalValues(source, copy);
        }
        else
        {
            foreach (var descriptor in descriptors)
            {
                descriptor.SetValue(copy, descriptor.CopyValue(descriptor.GetValue(source)));
            }
        }

        foreach (var palette in source.ColorPalettes)
        {
            copy.ColorPalettes[palette.Key] = CopyColorMap(palette.Value);
        }

        return copy;
    }

    private static void ApplyValues(
        DesignToken builder,
        IReadOnlyList<NormalizedTokenValue> values,
        TokenStage stage)
    {
        foreach (var value in values)
        {
            if (value.Descriptor.Stage == stage)
            {
                value.Descriptor.SetValue(builder, value.Descriptor.CopyValue(value.Value));
            }
        }
    }

    private static void ApplyAllValues(
        AbstractDesignToken builder,
        IReadOnlyList<NormalizedTokenValue> values)
    {
        foreach (var value in values)
        {
            value.Descriptor.SetValue(builder, value.Descriptor.CopyValue(value.Value));
        }
    }

    internal static DesignToken CreateBuilder(
        TokenValueTable values,
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> palettes,
        IReadOnlyList<TokenDescriptor> descriptors)
    {
        var canonical = IsCanonicalGlobalSchema(descriptors);
        var builder = new DesignToken(initializeSeed: !canonical);
        foreach (var descriptor in descriptors)
        {
            var value = values.GetValue(descriptor.Slot);
            descriptor.SetValue(builder, descriptor.CopyValue(value));
        }

        foreach (var palette in palettes)
        {
            builder.ColorPalettes[palette.Key] = ColorMap.FromColors(palette.Value.ColorSequence);
        }

        return builder;
    }

    private static void NormalizeFinalBehavior(DesignToken builder) => builder.NormalizeFinalBehavior();

    private static IReadOnlyDictionary<object, object?> BuildResourceMap(
        AbstractDesignToken builder,
        IReadOnlyList<TokenDescriptor> descriptors,
        TokenValueTable? frozenValues = null)
    {
        var reuseValues = frozenValues is not null && IsCanonicalGlobalSchema(descriptors);
        var resources = new Dictionary<object, object?>(descriptors.Count);
        foreach (var descriptor in descriptors)
        {
            var value = reuseValues ? frozenValues!.GetValue(descriptor.Slot) : null;
            // Only exact generated getters/projectors and unchanged immutable values can share this read.
            // Other references and all custom projectors retain their original ownership/callback boundary.
            var projected = reuseValues && (descriptor.ValueType.IsValueType || value is FontFamily)
                ? ThemeResourceValue.Project(value)
                : descriptor.ProjectResourceValue(builder);
            resources.Add(
                descriptor.ResourceKey,
                FreezeResourceValue(projected));
        }

        return resources.ToFrozenDictionary();
    }

    private static object? FreezeResourceValue(object? value) => ThemeResourceValue.Freeze(value);

    private static IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> FreezePresetPalettes(
        DesignToken builder)
    {
        var palettes = new Dictionary<PresetPrimaryColor, PaletteInfo>(
            builder.ColorPalettes.Count);
        foreach (var palette in builder.ColorPalettes)
        {
            var colors = GetColors(palette.Value);
            palettes.Add(
                palette.Key,
                new PaletteInfo(palette.Value.Color6, colors));
        }

        return palettes.ToFrozenDictionary();
    }

    private static ColorMap CopyColorMap(ColorMap source)
    {
        // Preserve the base ColorMap shape even when the source is a derived record.
        return new ColorMap
        {
            Color1 = source.Color1,
            Color2 = source.Color2,
            Color3 = source.Color3,
            Color4 = source.Color4,
            Color5 = source.Color5,
            Color6 = source.Color6,
            Color7 = source.Color7,
            Color8 = source.Color8,
            Color9 = source.Color9,
            Color10 = source.Color10
        };
    }

    private static Avalonia.Media.Color[] GetColors(ColorMap source)
    {
        return
        [
            source.Color1,
            source.Color2,
            source.Color3,
            source.Color4,
            source.Color5,
            source.Color6,
            source.Color7,
            source.Color8,
            source.Color9,
            source.Color10
        ];
    }

    private static void AddError(List<ThemeDefinitionDiagnostic> diagnostics, string code, string message)
    {
        diagnostics.Add(new ThemeDefinitionDiagnostic(
            code,
            ThemeDefinitionDiagnosticSeverity.Error,
            string.Empty,
            0,
            0,
            CompilerPath,
            message));
    }

    private static ThemeCompileResult Failed(List<ThemeDefinitionDiagnostic> diagnostics)
    {
        return new ThemeCompileResult(null, CopyDiagnostics(diagnostics), null);
    }

    private static IReadOnlyList<ThemeDefinitionDiagnostic> CopyDiagnostics(
        List<ThemeDefinitionDiagnostic> diagnostics)
    {
        return Array.AsReadOnly(diagnostics.ToArray());
    }
}
