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
            // Unchanged global values keep the previous publish's instances (see BuildResourceMap).
            globalResources = BuildResourceMap(
                globalBuilder,
                input.Registry.GlobalTokens,
                globalValues,
                input.ReusableParent?.GlobalResources);
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
            !input.Registry.ExtendsGlobalSchemaOf(parent.Registry) ||
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
            var hasControlStage = input.Registry.ControlAlgorithms.Count != 0 && descriptor.HasOwnTokens &&
                                  HasControlStage(input.Registry, descriptor.Identity,
                                      ControlStageChain(config, input.EffectiveConfig.Algorithms));
            cells[slot] = new ControlSnapshotCell(source, descriptor, config, FindValueDonor(input, descriptor.Identity, slot),
                hasControlStage);
        }
        return new LazyControlSnapshots(cells);
    }

    /// <summary>
    /// The same control's materialized snapshot in the reusable parent, when the parent still numbers the slot
    /// identically; used only to keep the instances of unchanged resource values (never as a computed result).
    /// </summary>
    private static ControlThemeSnapshot? FindValueDonor(ThemeCompileInput input, ControlTokenIdentity identity, int slot)
    {
        var parent = input.ReusableParent;
        if (parent is null ||
            parent.RegistryRevision != input.Registry.Revision ||
            (uint)slot >= (uint)parent.Controls.Count ||
            parent.Registry.GetControlIdentity(slot) != identity)
        {
            return null;
        }
        return parent.Controls.GetCell(slot).MaterializedValue;
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
            // A configured control never uses the root's shared resources directly; an unconfigured one does unless
            // the scope chain runs a control token stage for it (its EvaluateGlobal may write its global copy).
            var usesRootSharedResources = !hasConfig &&
                (registry.ControlAlgorithms.Count == 0 ||
                 !HasControlStage(registry, identity, effectiveConfig.Algorithms));
            cells[slot] = new ControlSnapshotCell(source, () => registry.Controls[controlSlot],
                () => effectiveConfig.TryGetControl(identity, out var config) ? config : null, usesRootSharedResources,
                previousValue, previousConfig, FindValueDonor(input, identity, slot));
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

    // 由独立 lazy source 在首次查询时执行，不依赖历史快照。previous 仅作为值身份复用来源
    // （同一控件上一次物化的快照），不影响计算结果。
    // Spec 4.3, control compilation: the effective global token (the four ControlAlgorithmMode cases), then each
    // control stage's EvaluateGlobal on that copy, the scope's explicit Map/Alias values written back over the
    // tokens it wrote (scopeGlobalTokens; read only on the stage path, and not in Global/Custom mode), the
    // control-scoped global overrides, then the
    // Own tokens through the staged evaluation. A control whose chain has no control stage takes the original path
    // unchanged.
    internal static ControlThemeSnapshot CompileControl(
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config,
        IReadOnlyList<ThemeAlgorithmDescriptor> globalAlgorithms,
        ThemeAppearance globalAppearance,
        DesignToken globalBuilder,
        TokenValueTable globalValues,
        IReadOnlyDictionary<object, object?> globalResources,
        ThemeSchemaRegistry registry,
        ControlThemeSnapshot? previous = null,
        IReadOnlyList<NormalizedTokenValue>? scopeGlobalTokens = null)
    {
        var globalDescriptors = registry.GlobalTokens;
        // Takes the place of the stage array: still a single reference local, null on the path without stages.
        var stages = CreateControlStagePlan(registry, descriptor, config, globalAlgorithms);
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
                        globalAppearance,
                        globalDescriptors);
                    appearance = ResolveAppearance(globalAppearance, globalAlgorithms);
                    break;

                case ControlAlgorithmMode.Custom:
                    effectiveGlobalBuilder = ReevaluateControlGlobal(
                        effectiveGlobalBuilder,
                        config.GlobalTokens,
                        config.Algorithms,
                        globalAppearance,
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

        // Global and Custom modes re-derive the control's global tokens without the scope's explicit Map/Alias values
        // (the documented existing exception of spec 4.3), so nothing is written back there either.
        if (stages is not null &&
            RunControlGlobalStage(stages, config,
                config is { AlgorithmMode: ControlAlgorithmMode.Global or ControlAlgorithmMode.Custom }
                    ? null
                    : scopeGlobalTokens,
                appearance, effectiveGlobalBuilder))
        {
            effectiveGlobalMatchesRoot = false;
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
            var effectiveGlobalResources = BuildResourceMap(
                effectiveGlobalBuilder,
                globalDescriptors,
                effectiveGlobalValues,
                previous?.EffectiveGlobalResourceDelta);
            globalDelta = BuildTokenDelta(globalValues, effectiveGlobalValues);
            globalResourceDelta = BuildDenseResourceDelta(globalResources, effectiveGlobalResources);
        }

        var controlBuilder = descriptor.CreateBuilder();
        var controlTokenValues = TokenValueTable.Empty;
        IReadOnlyDictionary<object, object?> controlResources =
            FrozenDictionary<object, object?>.Empty;
        if (controlBuilder is not null && stages is not null)
        {
            controlBuilder.AssignEffectiveGlobalToken(effectiveGlobalBuilder);
            RunControlOwnStage(descriptor, controlBuilder, config, appearance, effectiveGlobalBuilder, stages);
            controlTokenValues = TokenValueTable.Freeze(controlBuilder, descriptor.OwnTokens);
            controlResources = BuildResourceMap(
                controlBuilder,
                descriptor.OwnTokens,
                previousResources: previous?.ControlResources);
        }
        else if (controlBuilder is not null)
        {
            controlBuilder.AssignEffectiveGlobalToken(effectiveGlobalBuilder);
            var ownTokens = config?.OwnTokens;
            if (ownTokens is not null)
            {
                // Explicit overrides are written and flagged before the evaluator runs so default
                // calculations can see them; they are written again afterwards so they still win.
                MarkExplicit(controlBuilder, ownTokens);
            }

            descriptor.Evaluate(controlBuilder, appearance);
            if (ownTokens is not null)
            {
                ApplyAllValues(controlBuilder, ownTokens);
            }

            controlTokenValues = TokenValueTable.Freeze(controlBuilder, descriptor.OwnTokens);
            controlResources = BuildResourceMap(
                controlBuilder,
                descriptor.OwnTokens,
                previousResources: previous?.ControlResources);
        }

        return new ControlThemeSnapshot(
            descriptor.Slot,
            appearance,
            globalDelta,
            globalResourceDelta,
            controlTokenValues,
            controlResources);
    }

    /// <summary>
    /// Returns the control token stages that run for <paramref name="descriptor"/>, in chain order, or
    /// <see langword="null"/> when none does. The chain is the control's own one in
    /// <see cref="ControlAlgorithmMode.Custom"/> mode and <paramref name="globalAlgorithms"/> (the scope chain)
    /// otherwise. A control without own tokens has no control stage.
    /// </summary>
    /// <remarks>
    /// With no control stage registered at all (the built-in themes) this returns on the first check: no lookup,
    /// no hashing, no allocation.
    /// </remarks>
    internal static ControlTokenAlgorithmDescriptor[]? ResolveControlStages(
        ThemeSchemaRegistry registry,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config,
        IReadOnlyList<ThemeAlgorithmDescriptor> globalAlgorithms)
    {
        if (registry.ControlAlgorithms.Count == 0 || !descriptor.HasOwnTokens)
        {
            return null;
        }

        return CollectControlStages(registry, descriptor.Identity, ControlStageChain(config, globalAlgorithms));
    }

    /// <summary>
    /// Whether any algorithm of <paramref name="chain"/> has a control stage for <paramref name="control"/>. Allocates
    /// nothing; the caller has established that the control has own tokens.
    /// </summary>
    private static bool HasControlStage(
        ThemeSchemaRegistry registry,
        ControlTokenIdentity control,
        IReadOnlyList<ThemeAlgorithmDescriptor> chain)
    {
        if (registry.ControlAlgorithms.Count == 0)
        {
            return false;
        }

        for (var index = 0; index < chain.Count; index++)
        {
            if (registry.TryGetControlAlgorithm(chain[index].Algorithm, control, out _))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The control stages of one control compilation and the algorithm context both of its parts share. The context
    /// is created by <see cref="RunControlGlobalStage"/>, once the control's appearance is known.
    /// </summary>
    private sealed class ControlStagePlan(
        ControlTokenAlgorithmDescriptor[] stages,
        IReadOnlyList<ThemeAlgorithmDescriptor> chain)
    {
        internal ControlTokenAlgorithmDescriptor[] Stages { get; } = stages;

        /// <summary>The chain the stages were resolved from.</summary>
        internal IReadOnlyList<ThemeAlgorithmDescriptor> Chain { get; } = chain;

        internal ThemeAlgorithmContext Context;
    }

    private static ControlStagePlan? CreateControlStagePlan(
        ThemeSchemaRegistry registry,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config,
        IReadOnlyList<ThemeAlgorithmDescriptor> globalAlgorithms)
    {
        var stages = ResolveControlStages(registry, descriptor, config, globalAlgorithms);
        return stages is null ? null : new ControlStagePlan(stages, ControlStageChain(config, globalAlgorithms));
    }

    private static ControlTokenAlgorithmDescriptor[]? CollectControlStages(
        ThemeSchemaRegistry registry,
        ControlTokenIdentity control,
        IReadOnlyList<ThemeAlgorithmDescriptor> chain)
    {
        List<ControlTokenAlgorithmDescriptor>? stages = null;
        for (var index = 0; index < chain.Count; index++)
        {
            if (registry.TryGetControlAlgorithm(chain[index].Algorithm, control, out var stage))
            {
                (stages ??= new List<ControlTokenAlgorithmDescriptor>(chain.Count - index)).Add(stage);
            }
        }

        return stages?.ToArray();
    }

    private static IReadOnlyList<ThemeAlgorithmDescriptor> ControlStageChain(
        NormalizedControlThemeConfig? config,
        IReadOnlyList<ThemeAlgorithmDescriptor> globalAlgorithms) =>
        config is { AlgorithmMode: ControlAlgorithmMode.Custom } ? config.Algorithms : globalAlgorithms;

    private static ThemeAlgorithmContext CreateAlgorithmContext(
        ThemeAppearance appearance,
        IReadOnlyList<ThemeAlgorithmDescriptor> chain)
    {
        var identities = new ThemeAlgorithm[chain.Count];
        for (var index = 0; index < identities.Length; index++)
        {
            identities[index] = chain[index].Algorithm;
        }

        // Wrapped read-only so a stage cannot change the chain later algorithms see; this runs only when a stage does.
        return new ThemeAlgorithmContext(appearance, Array.AsReadOnly(identities));
    }

    /// <summary>
    /// Runs <c>EvaluateGlobal</c> of every control stage, in chain order, on the control's own copy of its effective
    /// global token. Then, for each token a stage wrote, the scope's explicit value is written back when the theme
    /// sets that token and it is a Map or Alias token (seed tokens are inputs the Map stage already replaced, so an
    /// explicit seed is not restored); <paramref name="scopeGlobalTokens"/> is <see langword="null"/> in
    /// <see cref="ControlAlgorithmMode.Global"/> and <see cref="ControlAlgorithmMode.Custom"/> mode, where nothing is
    /// written back; finally the control-scoped global overrides are written again so they win
    /// over both. The writes take effect directly, as in <see cref="ControlAlgorithmMode.Disabled"/> mode: Map and
    /// Alias are not rerun.
    /// </summary>
    /// <returns>Whether any stage wrote a global token.</returns>
    private static bool RunControlGlobalStage(
        ControlStagePlan plan,
        NormalizedControlThemeConfig? config,
        IReadOnlyList<NormalizedTokenValue>? scopeGlobalTokens,
        ThemeAppearance appearance,
        DesignToken controlGlobal)
    {
        plan.Context = CreateAlgorithmContext(appearance, plan.Chain);
        IReadOnlyCollection<string> names;
        ControlTokenAlgorithmDescriptor? running = null;
        // One recording session for the whole chain: its result is the union of the names every stage wrote.
        controlGlobal.BeginRecordingWrites();
        try
        {
            foreach (var stage in plan.Stages)
            {
                running = stage;
                stage.EvaluateGlobal(in plan.Context, controlGlobal);
            }
        }
        catch (Exception exception)
        {
            throw new ThemeAlgorithmContractException(
                running!.Algorithm,
                null,
                ThemeAlgorithmContractViolationKind.Failed,
                $"Algorithm '{running.Algorithm}' failed in the {nameof(TokenStage.Control)} stage.",
                exception);
        }
        finally
        {
            names = controlGlobal.EndRecordingWrites();
        }

        if (names.Count != 0 && scopeGlobalTokens is { Count: > 0 })
        {
            RestoreExplicitDerivedValues(controlGlobal, names, scopeGlobalTokens);
        }
        if (config is not null)
        {
            ApplyAllValues(controlGlobal, config.GlobalTokens);
        }
        NormalizeFinalBehavior(controlGlobal);
        return names.Count != 0;
    }

    // Spec 4.3 priority "algorithm < theme explicit value": the scope's explicit Map and Alias values are exactly the
    // values the global compilation wrote into the root builder after the algorithms ran (ApplyValues at Map,
    // CalculateAlias at Alias), so writing them back undoes only what a control stage overwrote. Seed values are
    // skipped: they were consumed by the Map stage, and the compiled seed slot holds the algorithm's result.
    private static void RestoreExplicitDerivedValues(
        DesignToken controlGlobal,
        IReadOnlyCollection<string> writtenNames,
        IReadOnlyList<NormalizedTokenValue> scopeGlobalTokens)
    {
        foreach (var name in writtenNames)
        {
            for (var index = 0; index < scopeGlobalTokens.Count; index++)
            {
                var value = scopeGlobalTokens[index];
                var descriptor = value.Descriptor;
                if (descriptor.Stage is (TokenStage.Map or TokenStage.Alias) &&
                    string.Equals(name, descriptor.Name, StringComparison.Ordinal))
                {
                    descriptor.SetValue(controlGlobal, descriptor.CopyValue(value.Value));
                    break;
                }
            }
        }
    }

    // Kept separate from CompileControl: the lambdas capture locals, and the closure would otherwise be allocated on
    // entry to CompileControl, on the path without control stages too.
    private static void RunControlOwnStage(
        ControlTokenDescriptor descriptor,
        AbstractControlDesignToken controlBuilder,
        NormalizedControlThemeConfig? config,
        ThemeAppearance appearance,
        DesignToken effectiveGlobal,
        ControlStagePlan plan)
    {
        // The context RunControlGlobalStage created for this compilation; copied so the lambdas can capture it.
        var context = plan.Context;
        var stages = plan.Stages;
        var steps = new StagedAlgorithmStep[stages.Length];
        for (var index = 0; index < steps.Length; index++)
        {
            var stage = stages[index];
            steps[index] = new StagedAlgorithmStep(
                stage.Algorithm,
                token => EvaluateOwnStage(stage, in context, effectiveGlobal, (AbstractControlDesignToken)token));
        }

        var ownTokens = config?.OwnTokens ?? Array.Empty<NormalizedTokenValue>();
        StagedTokenEvaluation.Run(
            controlBuilder,
            nameof(TokenStage.Control),
            () => MarkExplicit(controlBuilder, ownTokens),
            () => descriptor.Evaluate(controlBuilder, appearance),
            () => ApplyAllValues(controlBuilder, ownTokens),
            controlBuilder.ResetExplicit,
            steps,
            name => descriptor.TryGetOwnToken(name, out var token) ? token : null,
            static token => token.Stage == TokenStage.Control,
            token => controlBuilder.MarkExplicit(token.Slot));
    }

    // EvaluateOwn may only read the effective global token: its writes there are recorded and reported as a stage
    // boundary violation. The exception passes through the staged evaluation unchanged.
    private static void EvaluateOwnStage(
        ControlTokenAlgorithmDescriptor stage,
        in ThemeAlgorithmContext context,
        DesignToken effectiveGlobal,
        AbstractControlDesignToken token)
    {
        IReadOnlyCollection<string> names;
        effectiveGlobal.BeginRecordingWrites();
        try
        {
            stage.EvaluateOwn(in context, effectiveGlobal, token);
        }
        finally
        {
            names = effectiveGlobal.EndRecordingWrites();
        }

        if (names.Count == 0)
        {
            return;
        }

        // Every write here is a violation; the first one recorded is reported.
        var first = names.First();
        throw new ThemeAlgorithmContractException(
            stage.Algorithm,
            first,
            ThemeAlgorithmContractViolationKind.StageBoundary,
            $"Algorithm '{stage.Algorithm}' wrote token '{first}' outside the {nameof(TokenStage.Control)} stage.");
    }

    private static DesignToken ReevaluateControlGlobal(
        DesignToken baseline,
        IReadOnlyList<NormalizedTokenValue> overrides,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms,
        ThemeAppearance globalAppearance,
        IReadOnlyList<TokenDescriptor> descriptors)
    {
        ApplyValues(baseline, overrides, TokenStage.Seed);
        var result = EvaluateAlgorithms(baseline, algorithms, descriptors, out var aliasAlgorithms);
        ApplyValues(result, overrides, TokenStage.Map);
        CalculateAlias(result, overrides, aliasAlgorithms, globalAppearance, algorithms, descriptors);
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
        var map = EvaluateAlgorithms(
            seed, input.EffectiveConfig.Algorithms, input.Registry.GlobalTokens, out var aliasAlgorithms);
        ApplyValues(map, input.EffectiveConfig.GlobalTokens, TokenStage.Map);
        CalculateAlias(
            map,
            input.EffectiveConfig.GlobalTokens,
            aliasAlgorithms,
            ThemeAppearance.Light,
            input.EffectiveConfig.Algorithms,
            input.Registry.GlobalTokens);
        NormalizeFinalBehavior(map);
        return map;
    }

    // Explicit Alias values are written and flagged before the default Alias calculation so the Alias
    // tokens derived from them follow; they are written again afterwards so they still win, and the
    // flags are cleared so they never leak into copies of the builder. A chain whose algorithms have no
    // Alias stage (the built-in ones) stops here: no object is created and the formulas run once.
    internal static void CalculateAlias(
        DesignToken builder,
        IReadOnlyList<NormalizedTokenValue> values,
        List<AliasStageAlgorithm>? aliasAlgorithms,
        ThemeAppearance baselineAppearance,
        IReadOnlyList<ThemeAlgorithmDescriptor> chain,
        IReadOnlyList<TokenDescriptor> descriptors)
    {
        if (aliasAlgorithms is null)
        {
            MarkExplicit(builder, values, TokenStage.Alias);
            builder.CalculateAliasTokenValues();
            ApplyValues(builder, values, TokenStage.Alias);
            builder.ResetExplicit();
            return;
        }

        RunAliasStage(builder, values, aliasAlgorithms, baselineAppearance, chain, descriptors);
    }

    // Kept separate from CalculateAlias: the lambdas capture parameters, and the closure would otherwise be
    // allocated on entry to CalculateAlias, on the built-in path too.
    private static void RunAliasStage(
        DesignToken builder,
        IReadOnlyList<NormalizedTokenValue> values,
        List<AliasStageAlgorithm> aliasAlgorithms,
        ThemeAppearance baselineAppearance,
        IReadOnlyList<ThemeAlgorithmDescriptor> chain,
        IReadOnlyList<TokenDescriptor> descriptors)
    {
        var context = CreateAlgorithmContext(ResolveAppearance(baselineAppearance, chain), chain);
        var steps = new StagedAlgorithmStep[aliasAlgorithms.Count];
        for (var index = 0; index < steps.Length; index++)
        {
            var instance = aliasAlgorithms[index].Instance;
            steps[index] = new StagedAlgorithmStep(
                aliasAlgorithms[index].Algorithm,
                token => instance.EvaluateAlias(in context, (DesignToken)token));
        }

        var tokensByName = GlobalTokenNames.GetValue(descriptors, static list => new GlobalTokenNameIndex(list));
        StagedTokenEvaluation.Run(
            builder,
            nameof(TokenStage.Alias),
            () => MarkExplicit(builder, values, TokenStage.Alias),
            builder.CalculateAliasTokenValues,
            () => ApplyValues(builder, values, TokenStage.Alias),
            builder.ResetExplicit,
            steps,
            tokensByName.Find,
            static descriptor => descriptor.Stage == TokenStage.Alias,
            descriptor => builder.MarkExplicit(descriptor.Slot));
    }

    /// <summary>
    /// An algorithm of the chain that implements the Alias stage, with the instance that ran its Map stage.
    /// </summary>
    internal readonly record struct AliasStageAlgorithm(ThemeAlgorithm Algorithm, IThemeAliasAlgorithm Instance);

    // Global descriptor lists are registry-owned and immutable; the name index is built only the first time an
    // Alias stage runs against a list, never on the built-in path.
    private static readonly ConditionalWeakTable<IReadOnlyList<TokenDescriptor>, GlobalTokenNameIndex>
        GlobalTokenNames = new();

    private sealed class GlobalTokenNameIndex(IReadOnlyList<TokenDescriptor> descriptors)
    {
        private readonly FrozenDictionary<string, TokenDescriptor> _byName =
            descriptors.ToFrozenDictionary(static descriptor => descriptor.Name, StringComparer.Ordinal);

        internal TokenDescriptor? Find(string name) => _byName.GetValueOrDefault(name);
    }

    /// <summary>
    /// Runs the Map stage of every algorithm of the chain. The instances that also implement
    /// <see cref="IThemeAliasAlgorithm"/> are returned in chain order for the Alias stage; the list is created
    /// only when there is one, so it stays <see langword="null"/> for the built-in algorithms.
    /// </summary>
    internal static DesignToken EvaluateAlgorithms(
        DesignToken effectiveSeed,
        IReadOnlyList<ThemeAlgorithmDescriptor> algorithms,
        IReadOnlyList<TokenDescriptor> descriptors,
        out List<AliasStageAlgorithm>? aliasAlgorithms)
    {
        aliasAlgorithms = null;
        DesignToken? previousMap = null;
        foreach (var algorithm in algorithms)
        {
            var nextMap = CopyBuilder(previousMap ?? effectiveSeed, descriptors);
            IThemeAlgorithm instance;
            // Every algorithm's failure, built-in or not, is reported like a failure of its Alias or control stage:
            // a ThemeAlgorithmContractException naming the algorithm. The try region costs nothing when no
            // exception is thrown.
            try
            {
                instance = algorithm.Create();
                instance.Evaluate(effectiveSeed, previousMap, nextMap);
            }
            catch (Exception exception) when (exception is not ThemeAlgorithmContractException)
            {
                throw new ThemeAlgorithmContractException(
                    algorithm.Algorithm,
                    null,
                    ThemeAlgorithmContractViolationKind.Failed,
                    $"Algorithm '{algorithm.Algorithm}' failed in the {nameof(TokenStage.Map)} stage.",
                    exception);
            }

            if (instance is IThemeAliasAlgorithm aliasInstance)
            {
                (aliasAlgorithms ??= new List<AliasStageAlgorithm>()).Add(
                    new AliasStageAlgorithm(algorithm.Algorithm, aliasInstance));
            }
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

    private static void MarkExplicit(
        DesignToken builder,
        IReadOnlyList<NormalizedTokenValue> values,
        TokenStage stage)
    {
        // Indexed loop: foreach over the interface would box an enumerator on every compile.
        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            if (value.Descriptor.Stage == stage)
            {
                value.Descriptor.SetValue(builder, value.Descriptor.CopyValue(value.Value));
                builder.MarkExplicit(value.Descriptor.Slot);
            }
        }
    }

    private static void MarkExplicit(
        AbstractControlDesignToken builder,
        IReadOnlyList<NormalizedTokenValue> values)
    {
        foreach (var value in values)
        {
            value.Descriptor.SetValue(builder, value.Descriptor.CopyValue(value.Value));
            builder.MarkExplicit(value.Descriptor.Slot);
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

    /// <summary>
    /// Projects and freezes the resource value of every descriptor. A value that equals the one
    /// <paramref name="previousResources"/> holds for the same key keeps that previous instance, so an unchanged
    /// Token keeps reference identity across publishes (Avalonia short-circuits an equal republish by
    /// <c>EqualityComparer&lt;T&gt;.Default</c>, and a same-reference value also skips the equality call) while a
    /// changed Token gets its new frozen value. Mutable values (a <c>SolidColorBrush</c> Token) compare by reference
    /// and are therefore always new, as before.
    /// </summary>
    private static IReadOnlyDictionary<object, object?> BuildResourceMap(
        AbstractDesignToken builder,
        IReadOnlyList<TokenDescriptor> descriptors,
        TokenValueTable? frozenValues = null,
        IReadOnlyDictionary<object, object?>? previousResources = null)
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
            var frozen = FreezeResourceValue(projected);
            if (previousResources is not null &&
                previousResources.TryGetValue(descriptor.ResourceKey, out var previous) &&
                Equals(previous, frozen))
            {
                frozen = previous;
            }
            resources.Add(descriptor.ResourceKey, frozen);
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
