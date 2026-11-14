using System.Globalization;
using AtomUI.Generated.AtomUICore;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme.ContractTests;

/// <summary>
/// One broken rule of the theme algorithm contract (spec 4.6).
/// </summary>
/// <param name="Check">The check that failed; one of the constants on <see cref="ThemeAlgorithmContract"/>.</param>
/// <param name="Algorithm">The algorithm that broke the rule.</param>
/// <param name="TokenName">
/// The token involved (a global token, or a control's own token for a control stage), or <see langword="null"/> when
/// the rule is not about one token.
/// </param>
/// <param name="Message">What was observed.</param>
public sealed record ThemeAlgorithmContractViolation(string Check, ThemeAlgorithm Algorithm, string? TokenName, string Message);

/// <summary>
/// What the contract verifier sees: the registered algorithms (the Core built-ins plus package algorithms), the
/// control token descriptors and the control token stages that go into the theme schema registry it compiles against.
/// </summary>
public sealed record ThemeAlgorithmContractInput(
    IReadOnlyList<ThemeAlgorithmDescriptor> Algorithms,
    IReadOnlyList<ControlTokenDescriptor> Controls)
{
    /// <summary>
    /// The registered control token stages. The stages of the verified algorithm whose control is in
    /// <see cref="Controls"/> (with own tokens) are checked too; the others stay registered but are not called.
    /// Empty by default.
    /// </summary>
    public IReadOnlyList<ControlTokenAlgorithmDescriptor> ControlAlgorithms { get; init; } =
        Array.Empty<ControlTokenAlgorithmDescriptor>();

    /// <summary>
    /// Runs the caller's registration (e.g. <c>builder =&gt; builder.UsePrimeNGThemes()</c>) against a fresh theme
    /// manager builder and captures the Core built-in algorithms followed by every algorithm the registration added
    /// through <see cref="IThemeManagerBuilder.AddThemeAlgorithms"/>, the control token descriptors of the control
    /// packages it added through <see cref="IThemeManagerBuilder.AddControlPackage"/>, and the control token stages
    /// it added through <see cref="IThemeManagerBuilder.AddControlTokenAlgorithms"/>.
    /// </summary>
    /// <remarks>
    /// No Avalonia application is required: the builder is only constructed and registered against, never built.
    /// Control packages enabled through the application builder (<c>IAtomUIBuilder</c>) do not pass through
    /// <see cref="IThemeManagerBuilder"/> and are not captured; supply their descriptors with
    /// <c>input with { Controls = ... }</c>. The Map and Alias checks do not need controls; the control stage checks
    /// skip every stage whose control is not supplied.
    /// </remarks>
    public static ThemeAlgorithmContractInput Capture(Action<IThemeManagerBuilder> register)
    {
        ArgumentNullException.ThrowIfNull(register);
        var builder = new ThemeManagerBuilder();
        register(builder);
        builder.CompletePendingRegistrations();
        return new ThemeAlgorithmContractInput(
            GeneratedThemeSchema.GetAlgorithms().Concat(builder.ThemeAlgorithms).ToArray(),
            builder.ControlPackages.SelectMany(static package => package.Controls).ToArray())
        {
            ControlAlgorithms = builder.ControlTokenAlgorithms.ToArray()
        };
    }
}

/// <summary>
/// Reusable contract verifier for theme algorithms (spec 4.6). It runs one registered algorithm through the Map stage
/// directly and through the real theme compiler, and reports every rule it breaks.
/// </summary>
public static class ThemeAlgorithmContract
{
    /// <summary><c>Catalog</c> and <c>Id</c> are valid and registered exactly once; <c>Revision</c> is positive.</summary>
    public const string Identity = nameof(Identity);

    /// <summary>A standalone algorithm writes every Map-stage token.</summary>
    public const string MapComplete = nameof(MapComplete);

    /// <summary>
    /// The same input produces the same output, slot by slot: the global tokens, and for every control stage of the
    /// algorithm the control's own tokens and effective global tokens.
    /// </summary>
    public const string Deterministic = nameof(Deterministic);

    /// <summary>
    /// The Map stage leaves the effective seed and the previous map unchanged; a control stage's <c>EvaluateOwn</c>
    /// leaves the effective global token unchanged.
    /// </summary>
    public const string InputsUnchanged = nameof(InputsUnchanged);

    /// <summary>Changing the <c>ColorPrimary</c> seed changes the primary color and its states.</summary>
    public const string SeedResponsive = nameof(SeedResponsive);

    /// <summary>Under the default seed the primary color, its hover and its active state differ pairwise.</summary>
    public const string StatesDistinct = nameof(StatesDistinct);

    /// <summary>Body text and text on the primary fill meet the contrast thresholds of <see cref="ContrastRatio"/>.</summary>
    public const string Readable = nameof(Readable);

    /// <summary>A Dark algorithm has a darker background base than text base; a Light algorithm the opposite.</summary>
    public const string AppearanceConsistent = nameof(AppearanceConsistent);

    /// <summary>Appending <c>AtomUI:Compact</c> lowers the control heights and leaves color tokens unchanged.</summary>
    public const string ComposableWithCompact = nameof(ComposableWithCompact);

    /// <summary>
    /// Appending a built-in algorithm of the other appearance keeps the result readable: <c>AtomUI:Dark</c> after a
    /// Light or Preserve algorithm, <c>AtomUI:Default</c> after a Dark one. The check is skipped when that built-in is
    /// already in the verified chain, since a chain may not repeat an identity. In particular a Dark follower, verified
    /// as <c>[AtomUI:Default, X]</c>, has no hybrid form; that chain is already a light base followed by a dark
    /// algorithm and <see cref="Readable"/> covers it.
    /// </summary>
    public const string HybridChainReadable = nameof(HybridChainReadable);

    /// <summary>
    /// The compiler's staged evaluation stabilizes within its round limit, for the Alias stage and for the Own tokens
    /// of every control stage of the algorithm.
    /// </summary>
    public const string Stable = nameof(Stable);

    /// <summary>
    /// The Alias stage writes only Alias-stage tokens; a control stage's <c>EvaluateOwn</c> writes only the control's
    /// own tokens and never the effective global token.
    /// </summary>
    public const string StageBoundary = nameof(StageBoundary);

    /// <summary>
    /// The <c>ColorPrimary</c> seed that <see cref="SeedResponsive"/> compares against the default seed.
    /// </summary>
    public static readonly Color AlternativePrimarySeed = Color.FromRgb(0x72, 0x2E, 0xD1);

    private static readonly string[] s_primaryStates =
    [
        nameof(DesignToken.ColorPrimary),
        nameof(DesignToken.ColorPrimaryHover),
        nameof(DesignToken.ColorPrimaryActive)
    ];

    private static readonly string[] s_seedResponsiveTokens =
    [
        nameof(DesignToken.ColorPrimary),
        nameof(DesignToken.ColorPrimaryHover),
        nameof(DesignToken.ColorPrimaryActive),
        nameof(DesignToken.ColorPrimaryBg)
    ];

    private static readonly string[] s_controlHeights =
    [
        nameof(DesignToken.ControlHeight),
        nameof(DesignToken.ControlHeightSM),
        nameof(DesignToken.ControlHeightLG),
        nameof(DesignToken.ControlHeightXS)
    ];

    /// <summary>
    /// Verifies <paramref name="algorithm"/> against every check of the contract and returns the violations; an empty
    /// list means the algorithm satisfies the contract.
    /// </summary>
    /// <param name="input">The registered algorithms and controls, usually from <see cref="ThemeAlgorithmContractInput.Capture"/>.</param>
    /// <param name="algorithm">The algorithm to verify; it must be in <paramref name="input"/>.</param>
    /// <param name="standalone">
    /// <see langword="true"/> for an algorithm meant to start a chain: it runs alone and must write every Map-stage
    /// token. <see langword="false"/> for an algorithm meant to follow another one (<c>AtomUI:Dark</c>,
    /// <c>AtomUI:Compact</c>): it runs after <c>AtomUI:Default</c> and <see cref="MapComplete"/> is skipped.
    /// A Dark follower also skips <see cref="HybridChainReadable"/> (see there). <c>AtomUI:Default</c> cannot be
    /// verified as a follower.
    /// </param>
    /// <exception cref="ArgumentException"><paramref name="standalone"/> is false for <c>AtomUI:Default</c>.</exception>
    /// <exception cref="InvalidOperationException">
    /// The verification itself cannot proceed: a compilation fails for a reason that is not a contract violation,
    /// a built-in algorithm is missing from the input, or a Map-stage token has a value type without samples.
    /// </exception>
    public static IReadOnlyList<ThemeAlgorithmContractViolation> Verify(
        ThemeAlgorithmContractInput input,
        ThemeAlgorithm algorithm,
        bool standalone = true)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!standalone && algorithm == ThemeAlgorithm.Default)
        {
            throw new ArgumentException(
                "AtomUI:Default cannot be verified as a follower: it would follow itself.", nameof(standalone));
        }

        var verification = new Verification(algorithm);
        if (!verification.CheckIdentity(input))
        {
            return verification.Violations;
        }

        verification.Run(input, standalone);
        return verification.Violations;
    }

    private sealed class Verification(ThemeAlgorithm algorithm)
    {
        private readonly List<ThemeAlgorithmContractViolation> _violations = [];
        private ThemeSchemaRegistry _registry = null!;
        private ThemeAlgorithmDescriptor _descriptor = null!;
        private ThemeAlgorithmDescriptor _default = null!;
        private ThemeAlgorithmDescriptor _dark = null!;
        private ThemeAlgorithmDescriptor _compact = null!;
        private bool _standalone;
        private IReadOnlyList<TokenDescriptor> Globals => _registry.GlobalTokens;

        internal IReadOnlyList<ThemeAlgorithmContractViolation> Violations => _violations;

        internal bool CheckIdentity(ThemeAlgorithmContractInput input)
        {
            var matches = input.Algorithms.Where(descriptor => descriptor.Algorithm == algorithm).ToArray();
            if (matches.Length == 0)
            {
                Report(Identity, null, $"Algorithm '{algorithm}' is not registered.");
                return false;
            }

            if (matches.Length > 1)
            {
                Report(Identity, null, $"Algorithm '{algorithm}' is registered {matches.Length} times.");
                return false;
            }

            var valid = true;
            if (!SchemaIdentifier.IsValid(algorithm.Catalog))
            {
                Report(Identity, null, $"Catalog '{algorithm.Catalog}' is not a valid theme schema identifier.");
                valid = false;
            }

            if (!SchemaIdentifier.IsValid(algorithm.Id))
            {
                Report(Identity, null, $"Id '{algorithm.Id}' is not a valid theme schema identifier.");
                valid = false;
            }

            if (matches[0].Revision <= 0)
            {
                Report(Identity, null, $"Revision {matches[0].Revision} is not greater than 0.");
                valid = false;
            }

            return valid;
        }

        internal void Run(ThemeAlgorithmContractInput input, bool standalone)
        {
            _registry   = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), input.Controls,
                input.Algorithms, null, input.ControlAlgorithms);
            _descriptor = Resolve(algorithm);
            _default    = Resolve(ThemeAlgorithm.Default);
            _dark       = Resolve(ThemeAlgorithm.Dark);
            _compact    = Resolve(ThemeAlgorithm.Compact);
            _standalone = standalone;
            ThemeAlgorithmDescriptor[] chain = standalone ? [_descriptor] : [_default, _descriptor];

            if (standalone)
            {
                CheckMapComplete();
            }
            CheckMapDeterministic();
            CheckInputsUnchanged();

            var baseline = Compile(chain);
            if (baseline is not null)
            {
                CheckCompiledDeterministic(chain, baseline);
                CheckSeedResponsive(chain, baseline);
                CheckStatesDistinct(baseline);
                CheckReadable(Readable, baseline);
                CheckAppearanceConsistent(baseline);
                CheckComposableWithCompact(chain, baseline);
                CheckHybridChainReadable(chain);
            }
            CheckControlStages(chain);
        }

        private ThemeAlgorithmDescriptor Resolve(ThemeAlgorithm identity)
        {
            return _registry.TryGetAlgorithm(identity, out var descriptor)
                ? descriptor!
                : throw new InvalidOperationException(
                    $"Algorithm '{identity}' is not in the contract input; capture it with ThemeAlgorithmContractInput.Capture.");
        }

        // ---------------------------------------------------------------- Map stage, run directly

        private void CheckMapComplete()
        {
            var mapTokens = Globals.Where(static descriptor => descriptor.Stage == TokenStage.Map).ToArray();
            foreach (var descriptor in mapTokens)
            {
                MapTokenSamples.Get(descriptor);
            }

            var withA = RunMap(next => Prefill(next, mapTokens, first: true)).Next;
            var withB = RunMap(next => Prefill(next, mapTokens, first: false)).Next;
            foreach (var descriptor in mapTokens)
            {
                if (!SameValue(descriptor, descriptor.GetValue(withA), descriptor.GetValue(withB)))
                {
                    Report(MapComplete, descriptor.Name,
                        $"Map token '{descriptor.Name}' follows the value it held before the Map stage ran: " +
                        "the algorithm did not write it.");
                }
            }
        }

        private static void Prefill(DesignToken next, IReadOnlyList<TokenDescriptor> mapTokens, bool first)
        {
            foreach (var descriptor in mapTokens)
            {
                var (a, b) = MapTokenSamples.Get(descriptor);
                descriptor.SetValue(next, first ? a : b);
            }
        }

        private void CheckMapDeterministic()
        {
            var first = RunMap(null).Next;
            var second = RunMap(null).Next;
            foreach (var descriptor in Globals)
            {
                if (!SameValue(descriptor, descriptor.GetValue(first), descriptor.GetValue(second)))
                {
                    ReportNotDeterministic(descriptor.Name);
                }
            }
        }

        private void CheckInputsUnchanged()
        {
            var run = RunMap(null, (seed, previous) => (Capture(seed), previous is null ? null : Capture(previous)));
            CompareCapture(run.Seed, run.SeedBefore!, "effective seed");
            if (run.Previous is not null)
            {
                CompareCapture(run.Previous, run.PreviousBefore!, "previous map");
            }
        }

        private void CompareCapture(DesignToken token, object?[] before, string role, string stage = "The Map stage")
        {
            foreach (var descriptor in Globals)
            {
                if (!SameValue(descriptor, before[descriptor.Slot], descriptor.GetValue(token)))
                {
                    Report(InputsUnchanged, descriptor.Name,
                        $"{stage} changed token '{descriptor.Name}' of the {role}.");
                }
            }
        }

        private object?[] Capture(DesignToken token)
        {
            var values = new object?[Globals.Count];
            foreach (var descriptor in Globals)
            {
                values[descriptor.Slot] = descriptor.CopyValue(descriptor.GetValue(token));
            }

            return values;
        }

        private MapRun RunMap(
            Action<DesignToken>? prepareNext,
            Func<DesignToken, DesignToken?, (object?[] Seed, object?[]? Previous)>? captureInputs = null)
        {
            // The same preparation as the compiler: a fresh default seed, the previous algorithm's Map result, and a
            // next map copied from the previous map (or the seed).
            var seed = new DesignToken();
            var previous = _standalone
                ? null
                : ThemeCompiler.EvaluateAlgorithms(seed, [_default], Globals, out _);
            var next = Copy(previous ?? seed);
            prepareNext?.Invoke(next);
            var captured = captureInputs?.Invoke(seed, previous);
            _descriptor.Create().Evaluate(seed, previous, next);
            return new MapRun(seed, previous, next, captured?.Seed, captured?.Previous);
        }

        private DesignToken Copy(DesignToken source)
        {
            var copy = new DesignToken();
            foreach (var descriptor in Globals)
            {
                descriptor.SetValue(copy, descriptor.CopyValue(descriptor.GetValue(source)));
            }

            foreach (var palette in source.ColorPalettes)
            {
                copy.ColorPalettes[palette.Key] = palette.Value with { };
            }

            return copy;
        }

        private readonly record struct MapRun(
            DesignToken Seed,
            DesignToken? Previous,
            DesignToken Next,
            object?[]? SeedBefore,
            object?[]? PreviousBefore);

        // ---------------------------------------------------------------- Through the compiler

        private void CheckCompiledDeterministic(IReadOnlyList<ThemeAlgorithmDescriptor> chain, ThemeSnapshot baseline)
        {
            if (Compile(chain) is not { } again)
            {
                return;
            }

            foreach (var descriptor in Globals)
            {
                if (!SameValue(descriptor, Value(baseline, descriptor), Value(again, descriptor)))
                {
                    ReportNotDeterministic(descriptor.Name);
                }
            }
        }

        private void CheckSeedResponsive(IReadOnlyList<ThemeAlgorithmDescriptor> chain, ThemeSnapshot baseline)
        {
            if (Compile(chain, AlternativePrimarySeed) is not { } changed)
            {
                return;
            }

            foreach (var name in s_seedResponsiveTokens)
            {
                var descriptor = Global(name);
                if (SameValue(descriptor, Value(baseline, descriptor), Value(changed, descriptor)))
                {
                    Report(SeedResponsive, name,
                        $"Token '{name}' stayed {Format(baseline, descriptor)} when the ColorPrimary seed changed to " +
                        $"{AlternativePrimarySeed}.");
                }
            }
        }

        private void CheckStatesDistinct(ThemeSnapshot baseline)
        {
            for (var first = 0; first < s_primaryStates.Length; first++)
            {
                for (var second = first + 1; second < s_primaryStates.Length; second++)
                {
                    var left = Global(s_primaryStates[first]);
                    var right = Global(s_primaryStates[second]);
                    if (SameValue(left, Value(baseline, left), Value(baseline, right)))
                    {
                        Report(StatesDistinct, right.Name,
                            $"Tokens '{left.Name}' and '{right.Name}' are both {Format(baseline, left)} under the default seed.");
                    }
                }
            }
        }

        private void CheckReadable(string check, ThemeSnapshot snapshot)
        {
            var baseBackground = ColorOrNull(snapshot, nameof(DesignToken.ColorBgBase)) ??
                                 (snapshot.Appearance == ThemeAppearance.Dark ? Colors.Black : Colors.White);
            var container = ContrastRatio.Composite(
                ColorOf(snapshot, nameof(DesignToken.ColorBgContainer)), baseBackground);
            var primary = ContrastRatio.Composite(ColorOf(snapshot, nameof(DesignToken.ColorPrimary)), container);

            CheckContrast(check, snapshot, nameof(DesignToken.ColorText), container,
                nameof(DesignToken.ColorBgContainer), ContrastRatio.BodyText);
            CheckContrast(check, snapshot, nameof(DesignToken.ColorTextLightSolid), primary,
                nameof(DesignToken.ColorPrimary), ContrastRatio.TextOnSolid);
        }

        private void CheckContrast(
            string check,
            ThemeSnapshot snapshot,
            string foregroundName,
            Color background,
            string backgroundName,
            double threshold)
        {
            var foreground = ColorOf(snapshot, foregroundName);
            var ratio = ContrastRatio.Of(foreground, background);
            if (ratio < threshold)
            {
                Report(check, foregroundName, string.Format(CultureInfo.InvariantCulture,
                    "Contrast of '{0}' {1} on '{2}' {3} is {4:F2}, below {5} ({6} appearance).",
                    foregroundName, foreground, backgroundName, background, ratio, threshold, snapshot.Appearance));
            }
        }

        private void CheckAppearanceConsistent(ThemeSnapshot baseline)
        {
            if (_descriptor.AppearanceEffect == ThemeAppearanceEffect.Preserve)
            {
                return;
            }

            var background = ColorOrNull(baseline, nameof(DesignToken.ColorBgBase));
            var text = ColorOrNull(baseline, nameof(DesignToken.ColorTextBase));
            if (background is null || text is null)
            {
                Report(AppearanceConsistent, nameof(DesignToken.ColorBgBase),
                    "ColorBgBase or ColorTextBase is not set, so the appearance of the result cannot be told.");
                return;
            }

            var backgroundLuminance = ContrastRatio.RelativeLuminance(background.Value);
            var textLuminance = ContrastRatio.RelativeLuminance(text.Value);
            var consistent = _descriptor.AppearanceEffect == ThemeAppearanceEffect.Dark
                ? backgroundLuminance < textLuminance
                : backgroundLuminance > textLuminance;
            if (!consistent)
            {
                Report(AppearanceConsistent, nameof(DesignToken.ColorBgBase), string.Format(
                    CultureInfo.InvariantCulture,
                    "A {0} algorithm produced ColorBgBase {1} (luminance {2:F3}) against ColorTextBase {3} (luminance {4:F3}).",
                    _descriptor.AppearanceEffect, background.Value, backgroundLuminance, text.Value, textLuminance));
            }
        }

        private void CheckComposableWithCompact(IReadOnlyList<ThemeAlgorithmDescriptor> chain, ThemeSnapshot baseline)
        {
            if (chain.Contains(_compact) || Compile([.. chain, _compact]) is not { } compact)
            {
                return;
            }

            foreach (var name in s_controlHeights)
            {
                var descriptor = Global(name);
                var before = (double)Value(baseline, descriptor)!;
                var after = (double)Value(compact, descriptor)!;
                if (!(after < before))
                {
                    Report(ComposableWithCompact, name, string.Format(CultureInfo.InvariantCulture,
                        "Appending AtomUI:Compact did not lower '{0}': {1} before, {2} after.", name, before, after));
                }
            }

            foreach (var descriptor in Globals)
            {
                if (descriptor.ValueType != typeof(Color) && descriptor.ValueType != typeof(Color?))
                {
                    continue;
                }

                if (!SameValue(descriptor, Value(baseline, descriptor), Value(compact, descriptor)))
                {
                    Report(ComposableWithCompact, descriptor.Name,
                        $"Appending AtomUI:Compact changed color token '{descriptor.Name}' from " +
                        $"{Format(baseline, descriptor)} to {Format(compact, descriptor)}.");
                }
            }
        }

        private void CheckHybridChainReadable(IReadOnlyList<ThemeAlgorithmDescriptor> chain)
        {
            // The verified chain is Light unless the algorithm is Dark, so a Preserve algorithm (whose verified chain
            // is Light) is also checked under the Dark appearance: its Alias stage must handle both (spec 4.2).
            var opposite = _descriptor.AppearanceEffect == ThemeAppearanceEffect.Dark ? _default : _dark;
            // A chain may not hold the same identity twice, so a chain that already holds the opposite built-in
            // (a Dark follower verified after AtomUI:Default) has no hybrid form; Readable covers that chain.
            if (chain.Contains(opposite) || Compile([.. chain, opposite]) is not { } hybrid)
            {
                return;
            }

            CheckReadable(HybridChainReadable, hybrid);
        }

        // Compiles the chain through the real compiler. A ThemeAlgorithmContractException becomes a Stable or
        // StageBoundary violation and the caller skips its check; any other failure stops the verification.
        private ThemeSnapshot? Compile(IReadOnlyList<ThemeAlgorithmDescriptor> chain, Color? primarySeed = null)
        {
            var tokens = Array.Empty<NormalizedTokenValue>();
            if (primarySeed is { } seed)
            {
                var descriptor = Global(nameof(DesignToken.ColorPrimary));
                tokens = [new NormalizedTokenValue(descriptor, seed, descriptor.Format(seed))];
            }

            var definition = new PreparedThemeDefinition(
                "ThemeAlgorithmContract",
                "Theme Algorithm Contract",
                ThemeAppearance.Light,
                ThemeAppearance.Light,
                true,
                chain,
                Array.Empty<BoundTokenValue>(),
                Array.Empty<ControlThemeDefinition>(),
                new ThemeSourceLocation("theme-algorithm-contract", 1, 1, "/Theme"));
            var config = new NormalizedThemeConfig(
                false, true, chain, tokens, Array.Empty<NormalizedControlThemeConfig>());
            var result = new ThemeCompiler().Compile(new ThemeCompileInput(
                definition,
                new ThemeDefinitionRevision("theme-algorithm-contract", "1", "A1"),
                config,
                _registry));

            var chainText = ChainText(chain);
            switch (result.Exception)
            {
                case ThemeAlgorithmContractException { Violation: not ThemeAlgorithmContractViolationKind.Failed } contract:
                    ReportContract(contract);
                    return null;
                case { } exception:
                    throw new InvalidOperationException(
                        $"Compiling the chain [{chainText}] failed; the contract of '{algorithm}' cannot be verified.",
                        exception);
            }

            if (!result.Success)
            {
                throw new InvalidOperationException(
                    $"Compiling the chain [{chainText}] failed: " +
                    string.Join("; ", result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
            }

            return result.Snapshot!;
        }

        // The compiler says which rule was broken; the token may be a global or an own token.
        private void ReportContract(ThemeAlgorithmContractException contract)
        {
            var check = contract.Violation == ThemeAlgorithmContractViolationKind.Unstable ? Stable : StageBoundary;
            Report(check, contract.TokenName, contract.Message, contract.Algorithm);
        }

        private static string ChainText(IReadOnlyList<ThemeAlgorithmDescriptor> chain) =>
            string.Join(", ", chain.Select(static descriptor => descriptor.Algorithm.ToString()));

        // ---------------------------------------------------------------- Control stages, through the compiler

        // Every control stage of the verified algorithm whose control is registered with own tokens runs in the
        // verified chain (the control has no configuration, so the scope chain applies). A stage whose control was not
        // supplied cannot be called and is skipped.
        private void CheckControlStages(IReadOnlyList<ThemeAlgorithmDescriptor> chain)
        {
            var stages = new List<(ControlTokenAlgorithmDescriptor Stage, ControlTokenDescriptor Control)>();
            foreach (var stage in _registry.ControlAlgorithms)
            {
                if (stage.Algorithm == algorithm &&
                    _registry.TryGetControl(stage.Control, out var control) &&
                    control!.HasOwnTokens)
                {
                    stages.Add((stage, control));
                }
            }

            if (stages.Count == 0 || Compile(chain) is not { } first || Compile(chain) is not { } second)
            {
                return;
            }

            foreach (var (stage, control) in stages)
            {
                var left = Materialize(first, control, chain);
                CheckControlInputsUnchanged(stage, control, first, left, chain);
                if (left is null || Materialize(second, control, chain) is not { } right)
                {
                    continue;
                }

                foreach (var own in control.OwnTokens)
                {
                    if (!SameValue(own, left.ControlTokenValues.GetValue(own.Slot),
                            right.ControlTokenValues.GetValue(own.Slot)))
                    {
                        ReportNotDeterministic(own.Name);
                    }
                }

                foreach (var descriptor in Globals)
                {
                    if (!SameValue(descriptor, EffectiveGlobal(first, left, descriptor),
                            EffectiveGlobal(second, right, descriptor)))
                    {
                        ReportNotDeterministic(descriptor.Name);
                    }
                }
            }
        }

        // Compiling a control is lazy: its contract exception surfaces when the control's snapshot is read.
        private ControlThemeSnapshot? Materialize(
            ThemeSnapshot snapshot,
            ControlTokenDescriptor control,
            IReadOnlyList<ThemeAlgorithmDescriptor> chain)
        {
            try
            {
                return snapshot.Controls[control.Slot];
            }
            catch (ThemeAlgorithmContractException contract)
                when (contract.Violation != ThemeAlgorithmContractViolationKind.Failed)
            {
                ReportContract(contract);
                return null;
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Compiling control '{control.Identity}' under the chain [{ChainText(chain)}] failed; the contract " +
                    $"of '{algorithm}' cannot be verified.",
                    exception);
            }
        }

        // Runs EvaluateOwn directly on a copy of the control's effective global token, as the compiler hands it over,
        // and compares the copy before and after. When the control did not compile (a reported violation), the scope's
        // global token stands in for the effective one.
        private void CheckControlInputsUnchanged(
            ControlTokenAlgorithmDescriptor stage,
            ControlTokenDescriptor control,
            ThemeSnapshot snapshot,
            ControlThemeSnapshot? compiled,
            IReadOnlyList<ThemeAlgorithmDescriptor> chain)
        {
            var effectiveGlobal = ThemeCompiler.CreateBuilder(
                snapshot.GlobalTokenValues, snapshot.PresetColorPalettes, Globals);
            foreach (var descriptor in Globals)
            {
                if (compiled is not null &&
                    compiled.EffectiveGlobalTokenDelta.TryGetValue(descriptor.Slot, out var value))
                {
                    descriptor.SetValue(effectiveGlobal, descriptor.CopyValue(value));
                }
            }

            var appearance = compiled?.Appearance ?? snapshot.Appearance;
            var token = control.CreateBuilder()!;
            token.AssignEffectiveGlobalToken(effectiveGlobal);
            control.Evaluate(token, appearance);
            var before = Capture(effectiveGlobal);
            var context = new ThemeAlgorithmContext(
                appearance, chain.Select(static descriptor => descriptor.Algorithm).ToArray());
            try
            {
                stage.EvaluateOwn(in context, effectiveGlobal, token);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"The control stage of '{algorithm}' for '{control.Identity}' failed; its contract cannot be verified.",
                    exception);
            }

            CompareCapture(effectiveGlobal, before, "effective global token",
                $"The control stage for '{control.Identity}'");
        }

        private static object? EffectiveGlobal(
            ThemeSnapshot snapshot,
            ControlThemeSnapshot control,
            TokenDescriptor descriptor) =>
            control.EffectiveGlobalTokenDelta.TryGetValue(descriptor.Slot, out var value)
                ? value
                : snapshot.GlobalTokenValues.GetValue(descriptor.Slot);

        // ---------------------------------------------------------------- Helpers

        private TokenDescriptor Global(string name)
        {
            return _registry.TryGetGlobalToken(name, out var descriptor)
                ? descriptor!
                : throw new InvalidOperationException($"Global token '{name}' is not in the theme schema.");
        }

        private static object? Value(ThemeSnapshot snapshot, TokenDescriptor descriptor) =>
            snapshot.GlobalTokenValues.GetValue(descriptor.Slot);

        private static string Format(ThemeSnapshot snapshot, TokenDescriptor descriptor) =>
            Value(snapshot, descriptor) is { } value ? descriptor.Format(value) : "null";

        private Color ColorOf(ThemeSnapshot snapshot, string name) =>
            ColorOrNull(snapshot, name) ??
            throw new InvalidOperationException($"Global token '{name}' has no value.");

        private Color? ColorOrNull(ThemeSnapshot snapshot, string name) =>
            Value(snapshot, Global(name)) is Color color ? color : null;

        private static bool SameValue(TokenDescriptor descriptor, object? left, object? right)
        {
            if (Equals(left, right))
            {
                return true;
            }

            return left is not null && right is not null &&
                   string.Equals(descriptor.Format(left), descriptor.Format(right), StringComparison.Ordinal);
        }

        private void ReportNotDeterministic(string tokenName)
        {
            Report(Deterministic, tokenName,
                $"Two runs with the same input produced different values for token '{tokenName}'.");
        }

        private void Report(string check, string? tokenName, string message, ThemeAlgorithm? writer = null)
        {
            var violation = new ThemeAlgorithmContractViolation(check, writer ?? algorithm, tokenName, message);
            if (!_violations.Contains(violation))
            {
                _violations.Add(violation);
            }
        }
    }

    /// <summary>
    /// The two distinct prefill values per value type of a Map-stage token, used by <see cref="MapComplete"/>. A Map
    /// token of a type missing here fails the verification instead of being skipped.
    /// </summary>
    private static class MapTokenSamples
    {
        private static readonly Dictionary<Type, (object A, object B)> s_samples = new()
        {
            [typeof(Color)]        = (Color.FromArgb(0xFF, 0x12, 0x34, 0x56), Color.FromArgb(0x80, 0x65, 0x43, 0x21)),
            [typeof(double)]       = (1.25d, 2.75d),
            [typeof(CornerRadius)] = (new CornerRadius(1.25), new CornerRadius(2.75)),
            [typeof(Thickness)]    = (new Thickness(1.25), new Thickness(2.75)),
            [typeof(TimeSpan)]     = (TimeSpan.FromMilliseconds(125), TimeSpan.FromMilliseconds(275))
        };

        internal static (object A, object B) Get(TokenDescriptor descriptor)
        {
            return s_samples.TryGetValue(descriptor.ValueType, out var samples)
                ? samples
                : throw new InvalidOperationException(
                    $"No contract samples for value type '{descriptor.ValueType}' of Map token '{descriptor.Name}'. " +
                    "Add two distinct samples for it to ThemeAlgorithmContract.MapTokenSamples.");
        }
    }
}
