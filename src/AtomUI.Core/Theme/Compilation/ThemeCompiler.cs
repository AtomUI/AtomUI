using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using AtomUI.Generated.AtomUICore;
using Avalonia.Media;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Compilation;

/// <summary>
/// The single-pass pipeline of spec 6.2 for one evaluation unit (a scope): the language and variant, the seeds, the
/// language tokens, the global tokens, the core's final normalization and, lazily, every control's view. Explicit values
/// are inputs: seeds are written before the language derives anything, explicit language, Map, Alias and control
/// tokens are written and frozen before the step that would otherwise compute them (spec 6.3).
/// </summary>
internal sealed class ThemeCompiler
{
    private static readonly ConditionalWeakTable<IReadOnlyList<TokenDescriptor>, CanonicalGlobalSchema> GlobalSchemas = new();

    private const string CompilerPath = "ThemeCompiler";
    private const string MissingLanguageCode = "ATMTHM5001";
    private const string UnsupportedVariantCode = "ATMTHM5002";
    private const string UnknownLanguageTokenCode = "ATMTHM5003";

    private static readonly ConditionalWeakTable<IReadOnlyList<TokenDescriptor>, IReadOnlySet<string>> SeedNames = new();
    private const string SeedRejectReason = "a control token rule may override Map and Alias tokens only, not seeds.";
    private static readonly ConditionalWeakTable<PreparedThemeDefinition, NormalizedThemeConfig> s_definitionDefaults = new();
    private static readonly ConditionalWeakTable<ThemeSchemaRegistry, NormalizedThemeConfig> s_libraryDefaults = new();

    /// <summary>
    /// The lowest layer a theme definition gives: its language and fixed axes, and the definition itself as the value
    /// layer matched per evaluation unit.
    /// </summary>
    internal static NormalizedThemeConfig CreateDefinitionDefaults(
        PreparedThemeDefinition definition,
        ThemeSchemaRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(registry);
        return s_definitionDefaults.GetValue(definition, static value => NormalizedThemeConfig.CreateCanonical(
            false, value.Axes, Array.Empty<NormalizedTokenValue>(), Array.Empty<LanguageTokenValue>(),
            Array.Empty<NormalizedControlThemeConfig>(), value));
    }

    /// <summary>The layer of a scope with <c>Inherit = false</c>: nothing but the language it keeps from its parent.</summary>
    internal static NormalizedThemeConfig CreateLibraryDefaults(ThemeSchemaRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        return s_libraryDefaults.GetValue(registry, static _ => NormalizedThemeConfig.CreateCanonical(
            false, ThemeAxes.None, Array.Empty<NormalizedTokenValue>(), Array.Empty<LanguageTokenValue>(),
            Array.Empty<NormalizedControlThemeConfig>()));
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
            if (input.EffectiveConfig.Language is not { } language)
            {
                AddError(diagnostics, MissingLanguageCode, "A theme compilation requires a design language.");
                return Failed(diagnostics);
            }

            var variant = input.EffectiveConfig.Axes.ResolveVariant();
            if (!language.Axes.Supports(in variant))
            {
                AddError(diagnostics, UnsupportedVariantCode,
                    $"Design language '{language.Id}' does not support the variant '{variant}'.");
                return Failed(diagnostics);
            }

            if (preserveAppendedSlots && (input.ReusableParent is not { } previousInput ||
                previousInput.DefinitionRevision != input.DefinitionRevision ||
                !CanReuseGlobal(input, variant)))
            {
                throw new InvalidOperationException("Package append cannot change committed global theme inputs.");
            }
            var unit = UnitInputs.Create(input.EffectiveConfig, language, variant, diagnostics);
            var global = CompileGlobal(input, unit);
            var controls = CompileControls(input, unit, global, controlCompilationCache);
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
                input.Registry.Revision);

            return new ThemeCompileResult(
                new ThemeSnapshot(
                    input.Definition.Id,
                    input.Definition,
                    input.DefinitionRevision,
                    fingerprint,
                    variant,
                    input.Registry.Revision,
                    input.Registry,
                    input.EffectiveConfig,
                    global.Values,
                    global.LanguageValues,
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

    /// <summary>
    /// The value inputs of one evaluation unit: the definition layer matched for the unit's variant under the unit's
    /// explicit values (spec 7.7), the language tokens bound to the unit's language, and the per-control configuration.
    /// </summary>
    internal sealed class UnitInputs
    {
        private NormalizedControlThemeConfig[]? _controls;

        private UnitInputs(DesignLanguageDescriptor language, DesignVariant variant, NormalizedThemeConfig config,
            DefinitionLayer? layer, NormalizedTokenValue[] globalTokens, NormalizedTokenValue[] languageTokens)
        {
            Language       = language;
            Variant        = variant;
            Config         = config;
            Layer          = layer;
            GlobalTokens   = globalTokens;
            LanguageTokens = languageTokens;
        }

        internal DesignLanguageDescriptor Language { get; }
        internal DesignVariant Variant { get; }
        internal NormalizedThemeConfig Config { get; }
        internal DefinitionLayer? Layer { get; }

        /// <summary>The unit's explicit global tokens, definition layer included, ordered by slot.</summary>
        internal NormalizedTokenValue[] GlobalTokens { get; }

        /// <summary>The unit's explicit language tokens, bound to <see cref="Language"/>, ordered by slot.</summary>
        internal NormalizedTokenValue[] LanguageTokens { get; }

        internal static UnitInputs Create(NormalizedThemeConfig config, DesignLanguageDescriptor language,
            DesignVariant variant, List<ThemeDefinitionDiagnostic>? diagnostics)
        {
            var layer = config.Definition?.ValuesFor(in variant);
            var globalTokens = Overlay(layer?.GlobalTokens, config.GlobalTokens);
            var languageTokens = new Dictionary<int, NormalizedTokenValue>();
            if (layer is not null && ReferenceEquals(config.Definition!.Language, language))
            {
                foreach (var token in layer.LanguageTokens)
                {
                    languageTokens[token.Descriptor.Slot] = token.Normalize();
                }
            }
            foreach (var token in config.LanguageTokens)
            {
                if (!language.TryGetLanguageToken(token.Name, out var descriptor))
                {
                    // Review Focus 3: an override written for another language is ignored, with a diagnostic.
                    AddWarning(diagnostics, UnknownLanguageTokenCode,
                        $"'{token.Name}' is not a language Token of '{language.Id}'; the override is ignored.");
                    continue;
                }

                try
                {
                    var value = descriptor.Parse(token.Value);
                    languageTokens[descriptor.Slot] = new NormalizedTokenValue(descriptor, value, descriptor.Format(value));
                }
                catch (Exception exception) when (exception is not OutOfMemoryException)
                {
                    AddWarning(diagnostics, UnknownLanguageTokenCode,
                        $"Language Token '{token.Name}' value '{token.Value}' is invalid for '{language.Id}'; the override is ignored.");
                }
            }

            var languageArray = languageTokens.Values.ToArray();
            Array.Sort(languageArray, static (left, right) => left.Descriptor.Slot.CompareTo(right.Descriptor.Slot));
            return new UnitInputs(language, variant, config, layer, globalTokens, languageArray);
        }

        /// <summary>The unit's controls: the definition layer's control entries under the explicit ones, by identity.</summary>
        internal NormalizedControlThemeConfig[] Controls => _controls ??= BuildControls();

        private NormalizedControlThemeConfig[] BuildControls()
        {
            if (Layer is null || Layer.Controls.Count == 0)
            {
                return Config.Controls as NormalizedControlThemeConfig[] ?? Config.Controls.ToArray();
            }

            var merged = new Dictionary<ControlTokenIdentity, NormalizedControlThemeConfig>(Layer.Controls);
            foreach (var control in Config.Controls)
            {
                merged[control.Identity] = merged.TryGetValue(control.Identity, out var lower)
                    ? MergeControl(lower, control)
                    : control;
            }
            var result = merged.Values.ToArray();
            Array.Sort(result, static (left, right) => CompareIdentity(left.Identity, right.Identity));
            return result;
        }

        internal NormalizedControlThemeConfig? FindControl(ControlTokenIdentity identity)
        {
            var controls = Controls;
            var low = 0;
            var high = controls.Length - 1;
            while (low <= high)
            {
                var middle = (low + high) >>> 1;
                var comparison = CompareIdentity(controls[middle].Identity, identity);
                if (comparison == 0)
                {
                    return controls[middle];
                }
                if (comparison < 0)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle - 1;
                }
            }
            return null;
        }

        /// <summary>
        /// The inputs of a control that is its own evaluation unit: the scope's configuration matched for the control's
        /// variant (its fixed axes over the scope's), with the control's seed overrides as the highest seed layer.
        /// </summary>
        internal UnitInputs ForControlUnit(NormalizedControlThemeConfig control)
        {
            var variant = Variant with
            {
                Appearance = control.Appearance ?? Variant.Appearance,
                Density = control.Density ?? Variant.Density
            };
            var unit = variant == Variant ? this : Create(Config, Language, variant, null);
            var seeds = control.GlobalTokens.Where(static token => token.Descriptor.Stage == TokenStage.Seed).ToArray();
            return seeds.Length == 0
                ? unit
                : new UnitInputs(Language, variant, Config, unit.Layer, Overlay(unit.GlobalTokens, seeds), unit.LanguageTokens);
        }

        private static NormalizedControlThemeConfig MergeControl(NormalizedControlThemeConfig lower,
            NormalizedControlThemeConfig upper) =>
            NormalizedControlThemeConfig.CreateCanonical(
                ControlTokenIdentityCanonicalizer.Merge(lower.Identity, upper.Identity),
                upper.Appearance ?? lower.Appearance,
                upper.Density ?? lower.Density,
                Overlay(lower.GlobalTokens, upper.GlobalTokens),
                Overlay(lower.OwnTokens, upper.OwnTokens));
    }

    private readonly record struct GlobalCompileOutcome(
        DesignVariant Variant,
        bool ReuseGlobal,
        DesignToken? Builder,
        LanguageDesignToken? Language,
        TokenValueTable Values,
        TokenValueTable LanguageValues,
        IReadOnlyDictionary<object, object?> Resources,
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> Palettes);

    private static GlobalCompileOutcome CompileGlobal(ThemeCompileInput input, UnitInputs unit)
    {
        if (CanReuseGlobal(input, unit.Variant))
        {
            var parent = input.ReusableParent!;
            return new GlobalCompileOutcome(unit.Variant, true, null, null, parent.GlobalTokenValues,
                parent.LanguageTokenValues, parent.GlobalResources, parent.PresetColorPalettes);
        }

        var (builder, language) = CompileUnitGlobal(unit);
        var values = TokenValueTable.Freeze(builder, input.Registry.GlobalTokens);
        // Unchanged global values keep the previous publish's instances (see BuildResourceMap).
        var resources = BuildResourceMap(builder, input.Registry.GlobalTokens, values,
            input.ReusableParent?.GlobalResources);
        return new GlobalCompileOutcome(unit.Variant, false, builder, language, values,
            TokenValueTable.Freeze(language, unit.Language.LanguageTokens), resources, FreezePresetPalettes(builder));
    }

    /// <summary>Steps 2 to 5 of the pipeline for one evaluation unit: seeds, language tokens, global tokens, normalization.</summary>
    internal static (DesignToken Global, LanguageDesignToken Language) CompileUnitGlobal(UnitInputs unit)
    {
        var language = unit.Language;
        var variant = unit.Variant;
        var token = new DesignToken();
        var languageToken = language.CreateLanguageToken();

        // 2. Seeds: the language's defaults, then the explicit seeds of every layer (already overlaid).
        language.Language.DefineSeedsCore(in variant, token, languageToken);
        ApplyValues(token, unit.GlobalTokens, TokenStage.Seed);
        ApplyValues(languageToken, unit.LanguageTokens, TokenStage.Seed);

        // 3. Language tokens: explicit derived language tokens are frozen first.
        FreezeValues(languageToken, unit.LanguageTokens, TokenStage.Language);
        try
        {
            language.Language.DeriveLanguageCore(in variant, token, languageToken);
        }
        catch (Exception exception)
        {
            throw new ThemeDerivationException(language.Id, null, "language", exception);
        }

        // 4. Global tokens: explicit Map and Alias values are frozen first; seeds stay inputs the derivation may refine.
        FreezeValues(token, unit.GlobalTokens, TokenStage.Map);
        FreezeValues(token, unit.GlobalTokens, TokenStage.Alias);
        try
        {
            language.Language.DeriveGlobalCore(in variant, languageToken, token);
        }
        catch (Exception exception)
        {
            throw new ThemeDerivationException(language.Id, null, "global", exception);
        }

        // 5. The core's language-independent final normalization.
        token.ClearFreeze();
        languageToken.ClearFreeze();
        token.NormalizeFinalBehavior();
        return (token, languageToken);
    }

    private static bool CanReuseGlobal(ThemeCompileInput input, DesignVariant variant)
    {
        var parent = input.ReusableParent;
        if (parent is null ||
            !input.Registry.ExtendsGlobalSchemaOf(parent.Registry) ||
            parent.Variant != variant ||
            !ReferenceEquals(parent.EffectiveConfig.Language, input.EffectiveConfig.Language) ||
            !ReferenceEquals(parent.EffectiveConfig.Definition, input.EffectiveConfig.Definition))
        {
            return false;
        }

        if (input.ChangesFromReusableParent is { } changes)
        {
            return !changes.LanguageInputsChanged && !changes.GlobalTokensChanged;
        }

        return TokenSequenceEqual(parent.EffectiveConfig.GlobalTokens, input.EffectiveConfig.GlobalTokens) &&
               parent.EffectiveConfig.LanguageTokens.SequenceEqual(input.EffectiveConfig.LanguageTokens);
    }

    // 先按有序游标解析配置和复用状态，仅在首次查询时计算控件值与差量。
    private static LazyControlSnapshots CompileControls(
        ThemeCompileInput input,
        UnitInputs unit,
        GlobalCompileOutcome global,
        ControlCompilationCache? controlCompilationCache)
    {
        var count = input.Registry.Controls.Count;
        if (input.Registry.UsesStartupIndex)
        {
            return CompileIndexedControls(input, unit, global, controlCompilationCache, count);
        }
        var cells = new ControlSnapshotCell[count];
        var configIndex = 0;
        var parentConfigIndex = 0;
        var changedControlIndex = 0;
        var effectiveControls = unit.Controls;
        var changedGlobals = FindReadSetReuse(input, global);
        var previousControls = input.ReusableParent is { } parent && (global.ReuseGlobal || changedGlobals is not null)
            ? UnitInputs.Create(parent.EffectiveConfig, unit.Language, unit.Variant, null).Controls
            : null;
        LazyControlSnapshotSource? source = null;
        for (var slot = 0; slot < count; slot++)
        {
            var descriptor = input.Registry.Controls[slot];
            var config = FindControlConfig(
                effectiveControls,
                descriptor.Identity,
                ref configIndex);
            var parentConfig = previousControls is null
                ? null
                : FindControlConfig(
                    previousControls,
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
                    global.Variant,
                    controlChanged))
            {
                cells[slot] = input.ReusableParent!.Controls.GetCell(slot);
                continue;
            }
            if (changedGlobals is not null &&
                CanReuseByReads(input.ReusableParent!, slot, config, parentConfig, changedGlobals))
            {
                cells[slot] = input.ReusableParent!.Controls.GetCell(slot);
                continue;
            }
            if (config is null && !descriptor.HasOwnTokens)
            {
                cells[slot] = new ControlSnapshotCell(new ControlThemeSnapshot(
                    descriptor.Slot,
                    global.Variant.Appearance,
                    FrozenDictionary<int, object?>.Empty,
                    FrozenDictionary<object, object?>.Empty,
                    TokenValueTable.Empty,
                    FrozenDictionary<object, object?>.Empty));
                continue;
            }

            source ??= new LazyControlSnapshotSource(input, unit, global.Builder, global.Language, global.Values,
                global.LanguageValues, global.Resources, global.Palettes, controlCompilationCache);
            cells[slot] = new ControlSnapshotCell(source, descriptor, config, FindValueDonor(input, descriptor.Identity, slot));
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

    private static LazyControlSnapshots CompileIndexedControls(ThemeCompileInput input, UnitInputs unit,
        GlobalCompileOutcome global, ControlCompilationCache? cache, int count)
    {
        var cells = new ControlSnapshotCell[count];
        var parent = input.ReusableParent;
        var canReuse = parent is not null && ReferenceEquals(parent.Registry, input.Registry) &&
            parent.Variant == global.Variant && global.ReuseGlobal;
        var allControlsUnchanged = canReuse && input.EffectiveConfig.Equals(parent!.EffectiveConfig);
        var changedGlobals = FindReadSetReuse(input, global);
        var changedControls = canReuse && input.ChangesFromReusableParent is { } changes
            ? changes.ChangedControls.ToHashSet()
            : null;
        UnitInputs? parentUnit = null;
        LazyControlSnapshotSource? source = null;
        for (var slot = 0; slot < count; slot++)
        {
            var identity = input.Registry.GetControlIdentity(slot);
            if (allControlsUnchanged || (canReuse && changedControls is not null && !changedControls.Contains(identity)))
            {
                cells[slot] = parent!.Controls.GetCell(slot);
                continue;
            }
            var config = unit.FindControl(identity);
            if (changedGlobals is not null)
            {
                parentUnit ??= UnitInputs.Create(parent!.EffectiveConfig, unit.Language, unit.Variant, null);
                if (CanReuseByReads(parent!, slot, config, parentUnit.FindControl(identity), changedGlobals))
                {
                    cells[slot] = parent!.Controls.GetCell(slot);
                    continue;
                }
            }
            if (config is null && !input.Registry.ControlHasOwnTokens(slot))
            {
                cells[slot] = new ControlSnapshotCell(new ControlThemeSnapshot(slot, global.Variant.Appearance,
                    FrozenDictionary<int, object?>.Empty, FrozenDictionary<object, object?>.Empty,
                    TokenValueTable.Empty, FrozenDictionary<object, object?>.Empty));
                continue;
            }
            source ??= new LazyControlSnapshotSource(input, unit, global.Builder, global.Language, global.Values,
                global.LanguageValues, global.Resources, global.Palettes, cache);
            ControlThemeSnapshot? previousValue = null;
            NormalizedControlThemeConfig? previousConfig = null;
            if (canReuse && parent!.Controls.GetCell(slot).MaterializedValue is { } materialized)
            {
                previousValue = materialized;
                parentUnit ??= UnitInputs.Create(parent.EffectiveConfig, unit.Language, unit.Variant, null);
                previousConfig = parentUnit.FindControl(identity);
            }
            var controlSlot = slot;
            var registry = input.Registry;
            cells[slot] = new ControlSnapshotCell(source, () => registry.Controls[controlSlot], () => config,
                previousValue, previousConfig, FindValueDonor(input, identity, slot));
        }
        return new LazyControlSnapshots(cells);
    }

    /// <summary>
    /// Spec 12.3: the global slots a child unit changed relative to its parent, when the parent's control results may be
    /// reused by read set. <see langword="null"/> when they may not: no parent, a different registry, variant, language or
    /// definition, a changed seed, palette or language token (then every value may differ), or a reused global layer
    /// (the ordinary reuse applies).
    /// </summary>
    private static GlobalTokenReadSet? FindReadSetReuse(ThemeCompileInput input, GlobalCompileOutcome global)
    {
        var parent = input.ReusableParent;
        if (global.ReuseGlobal || parent is null ||
            parent.RegistryRevision != input.Registry.Revision ||
            !ReferenceEquals(parent.Registry, input.Registry) ||
            parent.Variant != global.Variant ||
            !ReferenceEquals(parent.EffectiveConfig.Language, input.EffectiveConfig.Language) ||
            !ReferenceEquals(parent.EffectiveConfig.Definition, input.EffectiveConfig.Definition) ||
            !SameValues(parent.LanguageTokenValues, global.LanguageValues) ||
            !SamePalettes(parent.PresetColorPalettes, global.Palettes))
        {
            return null;
        }

        var changes = GlobalTokenReadSet.Changes(parent.GlobalTokenValues, global.Values);
        foreach (var descriptor in input.Registry.GlobalTokens)
        {
            if (descriptor.Stage == TokenStage.Seed && changes.Contains(descriptor.Slot))
            {
                return null;
            }
        }

        return changes;
    }

    private static bool CanReuseByReads(ThemeSnapshot parent, int slot, NormalizedControlThemeConfig? config,
        NormalizedControlThemeConfig? parentConfig, GlobalTokenReadSet changedGlobals)
    {
        if ((uint)slot >= (uint)parent.Controls.Count ||
            parent.Controls.GetCell(slot).MaterializedValue is not { GlobalReads: { } reads } ||
            !(config is null ? parentConfig is null : config.Equals(parentConfig)))
        {
            return false;
        }

        return !reads.Overlaps(changedGlobals);
    }

    private static bool SameValues(TokenValueTable left, TokenValueTable right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }
        if (left.Count != right.Count)
        {
            return false;
        }
        for (var slot = 0; slot < left.Count; slot++)
        {
            if (!left.ValueEquals(slot, right.GetValue(slot)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SamePalettes(IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> left,
        IReadOnlyDictionary<PresetPrimaryColor, PaletteInfo> right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }
        if (left.Count != right.Count)
        {
            return false;
        }
        foreach (var (key, palette) in left)
        {
            if (!right.TryGetValue(key, out var other) || palette.Primary != other.Primary ||
                !palette.ColorSequence.SequenceEqual(other.ColorSequence))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CanReuseControl(
        ThemeSnapshot? parent,
        ThemeSchemaRegistry registry,
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config,
        NormalizedControlThemeConfig? parentConfig,
        bool reuseGlobal,
        DesignVariant variant,
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
            return parent.Variant.Appearance == variant.Appearance;
        }

        if (!reuseGlobal)
        {
            return false;
        }

        return controlChanged == false ||
               (config is null ? parentConfig is null : config.Equals(parentConfig));
    }

    /// <summary>
    /// Step 6 for one control: its view. The view's global tokens start as the scope's; the control's explicit global
    /// overrides are written and frozen, its explicit control tokens likewise, then the control token rule writes the
    /// control token and any global token that differs for this control. A control that fixes an axis or overrides a
    /// seed first gets its own global evaluation (spec 6.4).
    /// </summary>
    internal static ControlThemeSnapshot CompileControl(
        ControlTokenDescriptor descriptor,
        NormalizedControlThemeConfig? config,
        UnitInputs unit,
        DesignToken globalBuilder,
        LanguageDesignToken languageToken,
        TokenValueTable globalValues,
        IReadOnlyDictionary<object, object?> globalResources,
        ThemeSchemaRegistry registry,
        ControlThemeSnapshot? previous = null,
        ControlGlobalTokenPool? globalPool = null)
    {
        var globalDescriptors = registry.GlobalTokens;
        var appearance = unit.Variant.Appearance;
        var variant = unit.Variant;
        DesignToken controlGlobal;
        var effectiveGlobalMatchesRoot = true;
        if (config is { IsOwnEvaluationUnit: true })
        {
            var controlUnit = unit.ForControlUnit(config);
            (controlGlobal, languageToken) = CompileUnitGlobal(controlUnit);
            appearance = controlUnit.Variant.Appearance;
            variant = controlUnit.Variant;
            effectiveGlobalMatchesRoot = false;
        }
        else
        {
            controlGlobal = globalPool?.Rent() ?? CopyBuilder(globalBuilder, globalDescriptors);
        }

        // The control's explicit global overrides are what the control sees; normalized before they are frozen, as a
        // scope's are. A seed override made the control its own evaluation unit, so its Map and Alias tokens were
        // derived from the seed; the seed itself is written again because the language's global derivation may refine
        // or replace a seed input (a shadcn style's control height), and a control-level value is final for the control.
        var overrides = config?.GlobalTokens;
        if (overrides is { Count: > 0 })
        {
            foreach (var value in overrides)
            {
                value.Descriptor.SetValue(controlGlobal, value.Descriptor.CopyValue(value.Value));
            }
            controlGlobal.NormalizeFinalBehavior();
            effectiveGlobalMatchesRoot = false;

            // Every explicit value of the control is frozen against its token rule, seeds included (spec 6.3, 6.4).
            FreezeOverrides(controlGlobal, overrides);
        }

        var controlBuilder = descriptor.CreateBuilder();
        HashSet<string>? reads = null;
        var controlTokenValues = TokenValueTable.Empty;
        IReadOnlyDictionary<object, object?> controlResources = FrozenDictionary<object, object?>.Empty;
        if (controlBuilder is not null)
        {
            var ownTokens = config?.OwnTokens;
            if (ownTokens is { Count: > 0 })
            {
                foreach (var value in ownTokens)
                {
                    value.Descriptor.SetValue(controlBuilder, value.Descriptor.CopyValue(value.Value));
                    controlBuilder.Freeze(value.Descriptor.Name);
                }
            }

            var rule = registry.FindControlTokenRule(unit.Language, descriptor.Slot) ??
                             throw new ThemeDerivationException(unit.Language.Id, descriptor.Identity, "control",
                                 new InvalidOperationException(
                                     $"No control token rule is registered for '{descriptor.Identity}' under '{unit.Language.Id}'."));
            controlGlobal.RejectWrites(GetSeedNames(globalDescriptors), SeedRejectReason);
            controlGlobal.TrackWrites();
            var recordReads = config is not { IsOwnEvaluationUnit: true } && overrides is not { Count: > 0 };
            if (recordReads)
            {
                reads = new HashSet<string>(StringComparer.Ordinal);
                controlGlobal.RecordReads(reads);
            }
            try
            {
                rule.Apply(in variant, languageToken, controlBuilder, controlGlobal);
            }
            catch (Exception exception)
            {
                throw new ThemeDerivationException(unit.Language.Id, descriptor.Identity, "control", exception);
            }
            finally
            {
                controlGlobal.StopRecordingReads();
            }

            if (controlGlobal.HasAcceptedWrites)
            {
                effectiveGlobalMatchesRoot = false;
            }
            controlGlobal.ClearFreeze();
            controlBuilder.ClearFreeze();
            if (!effectiveGlobalMatchesRoot)
            {
                controlGlobal.NormalizeFinalBehavior();
            }

            controlTokenValues = TokenValueTable.Freeze(controlBuilder, descriptor.OwnTokens);
            controlResources = BuildResourceMap(
                controlBuilder,
                descriptor.OwnTokens,
                previousResources: previous?.ControlResources);
        }
        else
        {
            controlGlobal.ClearFreeze();
        }

        IReadOnlyDictionary<int, object?> globalDelta;
        IReadOnlyDictionary<object, object?> globalResourceDelta;
        if (effectiveGlobalMatchesRoot)
        {
            // Nothing was written to the view: it still equals the scope's global tokens and serves the next control.
            if (config is not { IsOwnEvaluationUnit: true })
            {
                globalPool?.Return(controlGlobal);
            }
            globalDelta = FrozenDictionary<int, object?>.Empty;
            globalResourceDelta = FrozenDictionary<object, object?>.Empty;
        }
        else
        {
            var effectiveGlobalValues = TokenValueTable.Freeze(controlGlobal, globalDescriptors);
            var effectiveGlobalResources = BuildResourceMap(
                controlGlobal,
                globalDescriptors,
                effectiveGlobalValues,
                previous?.EffectiveGlobalResourceDelta);
            globalDelta = BuildTokenDelta(globalValues, effectiveGlobalValues);
            globalResourceDelta = BuildDenseResourceDelta(globalResources, effectiveGlobalResources);
        }

        // The read set is kept only when the result depends on nothing but the scope's global tokens.
        var globalReads = effectiveGlobalMatchesRoot && reads is not null
            ? GlobalTokenReadSet.FromNames(reads, globalDescriptors)
            : null;
        return new ControlThemeSnapshot(
            descriptor.Slot,
            appearance,
            globalDelta,
            globalResourceDelta,
            controlTokenValues,
            controlResources,
            globalReads);
    }

    private static void FreezeOverrides(DesignToken controlGlobal, IReadOnlyList<NormalizedTokenValue> overrides)
    {
        foreach (var value in overrides)
        {
            controlGlobal.Freeze(value.Descriptor.Name);
        }
    }

    private static IReadOnlySet<string> GetSeedNames(IReadOnlyList<TokenDescriptor> descriptors) =>
        SeedNames.GetValue(descriptors, static list => list.Where(static token => token.Stage == TokenStage.Seed)
                                                           .Select(static token => token.Name)
                                                           .ToFrozenSet(StringComparer.Ordinal));

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

    /// <summary><paramref name="upper"/>'s values over <paramref name="lower"/>'s, by slot; both are ordered by slot.</summary>
    internal static NormalizedTokenValue[] Overlay(
        IReadOnlyList<NormalizedTokenValue>? lower,
        IReadOnlyList<NormalizedTokenValue> upper)
    {
        if (lower is null || lower.Count == 0)
        {
            return upper as NormalizedTokenValue[] ?? upper.ToArray();
        }
        if (upper.Count == 0)
        {
            return lower as NormalizedTokenValue[] ?? lower.ToArray();
        }

        var result = new List<NormalizedTokenValue>(lower.Count + upper.Count);
        int lowerIndex = 0, upperIndex = 0;
        while (lowerIndex < lower.Count || upperIndex < upper.Count)
        {
            if (lowerIndex == lower.Count)
            {
                result.Add(upper[upperIndex++]);
                continue;
            }
            if (upperIndex == upper.Count)
            {
                result.Add(lower[lowerIndex++]);
                continue;
            }

            var comparison = lower[lowerIndex].Descriptor.Slot.CompareTo(upper[upperIndex].Descriptor.Slot);
            if (comparison < 0)
            {
                result.Add(lower[lowerIndex++]);
            }
            else
            {
                result.Add(upper[upperIndex++]);
                if (comparison == 0)
                {
                    lowerIndex++;
                }
            }
        }

        return result.ToArray();
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

    internal static DesignToken CopyBuilder(
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
        AbstractDesignToken builder,
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
            }
        }
    }

    private static void FreezeValues(
        AbstractDesignToken builder,
        IReadOnlyList<NormalizedTokenValue> values,
        TokenStage stage)
    {
        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            if (value.Descriptor.Stage == stage)
            {
                value.Descriptor.SetValue(builder, value.Descriptor.CopyValue(value.Value));
                builder.Freeze(value.Descriptor.Name);
            }
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

    /// <summary>Recreates a language token object from its frozen values (a scope that reuses its parent's global values).</summary>
    internal static LanguageDesignToken CreateLanguageBuilder(DesignLanguageDescriptor language, TokenValueTable values)
    {
        var builder = language.CreateLanguageToken();
        foreach (var descriptor in language.LanguageTokens)
        {
            if (descriptor.Slot < values.Count)
            {
                descriptor.SetValue(builder, descriptor.CopyValue(values.GetValue(descriptor.Slot)));
            }
        }

        return builder;
    }

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

    private static void AddWarning(List<ThemeDefinitionDiagnostic>? diagnostics, string code, string message)
    {
        diagnostics?.Add(new ThemeDefinitionDiagnostic(
            code,
            ThemeDefinitionDiagnosticSeverity.Warning,
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

/// <summary>Theme token evaluation failed. Identifies the language, the control (if any) and the step.</summary>
public sealed class ThemeDerivationException : Exception
{
    internal ThemeDerivationException(string language, ControlTokenIdentity? control, string step, Exception innerException)
        : base(control is { } identity
            ? $"Design language '{language}' failed to apply the token rule for control '{identity}'."
            : $"Design language '{language}' failed in its {step} derivation.", innerException)
    {
        Language = language;
        Control  = control;
    }

    public string Language { get; }

    public ControlTokenIdentity? Control { get; }
}
