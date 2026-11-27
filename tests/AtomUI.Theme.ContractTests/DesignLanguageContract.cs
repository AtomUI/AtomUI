using System.Globalization;
using AtomUI.Generated.AtomUICore;
using AtomUI.Registration;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme.ContractTests;

/// <summary>
/// One broken rule of the design language contract.
/// </summary>
/// <param name="Check">
/// The check that failed; one of the constants on <see cref="DesignLanguageContract" />.
/// </param>
/// <param name="Variant">
/// The variant the rule broke under, as <c>Style/Appearance/Density/Idiom</c>.
/// </param>
/// <param name="TokenName">
/// The token involved (a global, language or control token), or <see langword="null" /> when the rule is not about one
/// token.
/// </param>
/// <param name="Message">
/// What was observed.
/// </param>
public sealed record DesignLanguageContractViolation(string Check, string Variant, string? TokenName, string Message);

/// <summary>
/// What the contract verifier sees: the languages, control packages and control token rules an application registration
/// enables, folded into the schema registry the theme compiler runs against.
/// </summary>
public sealed class DesignLanguageContractInput
{
    private DesignLanguageContractInput(ThemeSchemaRegistry registry) => Registry = registry;

    internal ThemeSchemaRegistry Registry { get; }

    /// <summary>
    /// The identities of the enabled languages.
    /// </summary>
    public IReadOnlyList<string> Languages => Registry.Languages.Select(static language => language.Id).ToArray();

    /// <summary>
    /// Runs the caller's registration (e.g. <c>builder =&gt; builder.UseAntDesign(l =&gt; l.AddDesktopControls())</c>)
    /// against a fresh application builder and captures the languages, the control packages and the control token rules
    /// it enables. No Avalonia application is required: the builder is registered against, never committed.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The registration does not cover every control under every language.
    /// </exception>
    public static DesignLanguageContractInput Capture(Action<IAtomUIBuilder> register)
    {
        ArgumentNullException.ThrowIfNull(register);
        var builder = new AtomUIBuilder(typeof(Application));
        register(builder);
        var theme = (ThemeManagerBuilder)builder.Theme;
        theme.CompletePendingRegistrations();
        var manualControls = theme.ControlPackages.SelectMany(static package => package.Controls).ToArray();
        var manualRegistry = manualControls.Length == 0
            ? null
            : new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), manualControls,
                Array.Empty<DesignLanguageDescriptor>());
        var index = StartupPackageIndex.Create(builder.PackageDeclarations, null, manualRegistry);
        // The registry checks that every control has a rule under every language.
        var registry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), index, theme.Languages,
            theme.TokenRules);
        return new DesignLanguageContractInput(registry);
    }
}

/// <summary>
/// Reusable contract verifier for design languages and their control values: purity, completeness of the three token
/// kinds, readability of paired roles, determinism under every variant. It runs the language's rules
/// directly for the completeness checks and through the real theme compiler for the others, under every declared
/// style × appearance × density × idiom.
/// </summary>
public static class DesignLanguageContract
{
    /// <summary>
    /// The language is registered exactly once with a valid identity and declares at least one value per axis.
    /// </summary>
    public const string Identity = nameof(Identity);

    /// <summary>
    /// <c>DeriveLanguage</c> writes every language token that is not a seed.
    /// </summary>
    public const string LanguageComplete = nameof(LanguageComplete);

    /// <summary>
    /// <c>DeriveGlobal</c> writes every Map and Alias token.
    /// </summary>
    public const string GlobalComplete = nameof(GlobalComplete);

    /// <summary>
    /// Every control token rule of the language writes every own token of its control.
    /// </summary>
    public const string ControlComplete = nameof(ControlComplete);

    /// <summary>
    /// The rules are pure: <c>DeriveGlobal</c> leaves the language token unchanged, a control token rule leaves the
    /// language token unchanged, and compiling other variants in between does not change a variant's result.
    /// </summary>
    public const string Pure = nameof(Pure);

    /// <summary>
    /// Two compilations of the same variant produce the same global, language and control token values.
    /// </summary>
    public const string Deterministic = nameof(Deterministic);

    /// <summary>
    /// Body text and text on the primary fill meet the contrast thresholds of <see cref="ContrastRatio" />.
    /// </summary>
    public const string Readable = nameof(Readable);

    /// <summary>
    /// Every variant the language declares: each style × Light/Dark × each density × each idiom.
    /// </summary>
    public static IReadOnlyList<DesignVariant> Variants(DesignLanguageAxes axes)
    {
        ArgumentNullException.ThrowIfNull(axes);
        return (from style in axes.Styles
                from appearance in new[] { ThemeAppearance.Light, ThemeAppearance.Dark }
                from density in axes.Densities
                from idiom in axes.Idioms
                select new DesignVariant(style, appearance, density, idiom)).ToArray();
    }

    /// <summary>
    /// Verifies the language <paramref name="languageId" /> and every control token rule registered for it against each
    /// check of the contract, and returns the violations; an empty list means the language satisfies the contract.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The verification itself cannot proceed: a compilation fails, or a token has a value type without samples.
    /// </exception>
    public static IReadOnlyList<DesignLanguageContractViolation> Verify(DesignLanguageContractInput input, string languageId)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrEmpty(languageId);
        var verification = new Verification(input.Registry, languageId);
        verification.Run();
        return verification.Violations;
    }

    private sealed class Verification(ThemeSchemaRegistry registry, string languageId)
    {
        private readonly List<DesignLanguageContractViolation> _violations = [];
        private DesignLanguageDescriptor _language = null!;
        private IReadOnlyList<TokenDescriptor> Globals => registry.GlobalTokens;

        internal IReadOnlyList<DesignLanguageContractViolation> Violations => _violations;

        internal void Run()
        {
            if (!CheckIdentity())
            {
                return;
            }

            var variants = Variants(_language.Axes);
            var rules = registry.ControlTokenRules
                                      .Where(rule => rule.LanguageTokenType.IsAssignableFrom(_language.LanguageTokenType))
                                      .Select(rule => registry.FindControlTokenRule(_language, Slot(rule)))
                                      .OfType<ControlTokenRuleDescriptor>()
                                      .Distinct()
                                      .ToArray();
            var first = new Dictionary<DesignVariant, Compiled>();
            foreach (var variant in variants)
            {
                CheckLanguageComplete(variant);
                var (_, language) = CheckGlobalComplete(variant);
                foreach (var rule in rules)
                {
                    CheckControlComplete(variant, rule, language);
                }

                var compiled = Compile(variant);
                first.Add(variant, compiled);
                CompareCompiled(Deterministic, variant, compiled, Compile(variant));
                CheckReadable(variant, compiled);
            }

            // Compiling the other variants in between must not change a variant's result.
            foreach (var variant in variants)
            {
                CompareCompiled(Pure, variant, first[variant], Compile(variant));
            }
        }

        private int Slot(ControlTokenRuleDescriptor rule) =>
            registry.TryGetControl(rule.Control, out var control)
                ? control!.Slot
                : throw new InvalidOperationException($"Control '{rule.Control}' is not registered.");

        private bool CheckIdentity()
        {
            var matches = registry.Languages.Where(language => language.Id == languageId).ToArray();
            if (matches.Length != 1)
            {
                Report(Identity, "-", null, matches.Length == 0
                    ? $"Design language '{languageId}' is not registered."
                    : $"Design language '{languageId}' is registered {matches.Length} times.");
                return false;
            }

            _language = matches[0];
            var valid = true;
            if (!SchemaIdentifier.IsValid(_language.Id))
            {
                Report(Identity, "-", null, $"Id '{_language.Id}' is not a valid theme schema identifier.");
                valid = false;
            }

            var axes = _language.Axes;
            if (axes.Styles.Count == 0 || axes.Densities.Count == 0 || axes.Idioms.Count == 0)
            {
                Report(Identity, "-", null, "The language declares no style, density or idiom.");
                valid = false;
            }
            else if (!axes.Styles.Contains(axes.DefaultStyle))
            {
                Report(Identity, "-", null, $"The default style '{axes.DefaultStyle}' is not among the declared styles.");
                valid = false;
            }

            return valid;
        }

        // ---------------------------------------------------------------- Run directly

        private (DesignToken Seed, LanguageDesignToken Language) Seed(in DesignVariant variant)
        {
            var seed = new DesignToken();
            var language = _language.CreateLanguageToken();
            _language.Language.DefineSeedsCore(variant, seed, language);
            return (seed, language);
        }

        private void CheckLanguageComplete(DesignVariant variant)
        {
            var derived = _language.LanguageTokens.Where(static token => token.Stage != TokenStage.Seed).ToArray();
            if (derived.Length == 0)
            {
                return;
            }

            LanguageDesignToken Run(bool first)
            {
                var (seed, language) = Seed(variant);
                Prefill(language, derived, first);
                _language.Language.DeriveLanguageCore(variant, seed, language);
                return language;
            }

            var withA = Run(true);
            var withB = Run(false);
            foreach (var descriptor in derived)
            {
                if (!SameValue(descriptor, descriptor.GetValue(withA), descriptor.GetValue(withB)))
                {
                    Report(LanguageComplete, variant, descriptor.Name,
                        $"Language token '{descriptor.Name}' follows the value it held before DeriveLanguage ran: it was not written.");
                }
            }
        }

        // Returns the global and language tokens of the variant as the compiler hands them to the control token rules.
        private (DesignToken Global, LanguageDesignToken Language) CheckGlobalComplete(DesignVariant variant)
        {
            var derived = Globals.Where(static token => token.Stage is TokenStage.Map or TokenStage.Alias).ToArray();

            (DesignToken Global, LanguageDesignToken Language, object?[] LanguageBefore) Run(bool? first)
            {
                var (token, language) = Seed(variant);
                _language.Language.DeriveLanguageCore(variant, token, language);
                if (first is { } a)
                {
                    Prefill(token, derived, a);
                }

                var before = Capture(language, _language.LanguageTokens);
                _language.Language.DeriveGlobalCore(variant, language, token);
                return (token, language, before);
            }

            var withA = Run(true);
            var withB = Run(false);
            foreach (var descriptor in derived)
            {
                if (!SameValue(descriptor, descriptor.GetValue(withA.Global), descriptor.GetValue(withB.Global)))
                {
                    Report(GlobalComplete, variant, descriptor.Name,
                        $"Global token '{descriptor.Name}' follows the value it held before DeriveGlobal ran: it was not written.");
                }
            }

            var plain = Run(null);
            CompareCapture(Pure, variant, plain.Language, _language.LanguageTokens, plain.LanguageBefore,
                "DeriveGlobal changed language token");
            plain.Global.NormalizeFinalBehavior();
            return (plain.Global, plain.Language);
        }

        private void CheckControlComplete(DesignVariant variant, ControlTokenRuleDescriptor rule,
            LanguageDesignToken language)
        {
            if (!registry.TryGetControl(rule.Control, out var control) || control!.OwnTokens.Count == 0)
            {
                return;
            }

            var (global, _) = CheckGlobalCompleteSilently(variant);
            (AbstractControlDesignToken Token, object?[] LanguageBefore, LanguageDesignToken Language) Run(bool first)
            {
                var token = control.CreateBuilder()!;
                Prefill(token, control.OwnTokens, first);
                var languageCopy = CopyLanguage(language);
                var before = Capture(languageCopy, _language.LanguageTokens);
                rule.Apply(variant, languageCopy, token, Copy(global));
                return (token, before, languageCopy);
            }

            var withA = Run(true);
            var withB = Run(false);
            foreach (var descriptor in control.OwnTokens)
            {
                if (!SameValue(descriptor, descriptor.GetValue(withA.Token), descriptor.GetValue(withB.Token)))
                {
                    Report(ControlComplete, variant, $"{control.Identity}.{descriptor.Name}",
                        $"Control token '{control.Identity}.{descriptor.Name}' follows the value it held before the " +
                        "rule ran: it was not written.");
                }
            }

            CompareCapture(Pure, variant, withA.Language, _language.LanguageTokens, withA.LanguageBefore,
                $"The rule for '{control.Identity}' changed language token");
        }

        private readonly Dictionary<DesignVariant, DesignToken> _globals = new();

        private (DesignToken Global, LanguageDesignToken Language) CheckGlobalCompleteSilently(DesignVariant variant)
        {
            if (!_globals.TryGetValue(variant, out var global))
            {
                var (token, language) = Seed(variant);
                _language.Language.DeriveLanguageCore(variant, token, language);
                _language.Language.DeriveGlobalCore(variant, language, token);
                token.NormalizeFinalBehavior();
                _globals.Add(variant, global = token);
            }

            return (global, null!);
        }

        private LanguageDesignToken CopyLanguage(LanguageDesignToken source)
        {
            var copy = _language.CreateLanguageToken();
            foreach (var descriptor in _language.LanguageTokens)
            {
                descriptor.SetValue(copy, descriptor.CopyValue(descriptor.GetValue(source)));
            }

            return copy;
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

        private static void Prefill(AbstractDesignToken token, IEnumerable<TokenDescriptor> descriptors, bool first)
        {
            foreach (var descriptor in descriptors)
            {
                var (a, b) = TokenSamples.Get(descriptor);
                descriptor.SetValue(token, first ? a : b);
            }
        }

        private static object?[] Capture(AbstractDesignToken token, IReadOnlyList<TokenDescriptor> descriptors) =>
            descriptors.Select(descriptor => descriptor.CopyValue(descriptor.GetValue(token))).ToArray();

        private void CompareCapture(string check, DesignVariant variant, AbstractDesignToken token,
            IReadOnlyList<TokenDescriptor> descriptors, object?[] before, string message)
        {
            for (var i = 0; i < descriptors.Count; i++)
            {
                var descriptor = descriptors[i];
                if (!SameValue(descriptor, before[i], descriptor.GetValue(token)))
                {
                    Report(check, variant, descriptor.Name, $"{message} '{descriptor.Name}'.");
                }
            }
        }

        // ---------------------------------------------------------------- Through the compiler

        private sealed record Compiled(ThemeSnapshot Snapshot, IReadOnlyDictionary<ControlTokenIdentity, ControlThemeSnapshot> Controls);

        private Compiled Compile(DesignVariant variant)
        {
            var config = new ThemeConfigBuilder().WithLanguage(_language.Id).WithStyle(variant.Style)
                                                 .WithAppearance(variant.Appearance).WithDensity(variant.Density)
                                                 .WithIdiom(variant.Idiom).Build();
            var normalized = ThemeConfigNormalizer.Normalize(config, registry);
            if (!normalized.Success)
            {
                throw new InvalidOperationException($"The configuration of variant {Text(variant)} is invalid: " +
                                                    string.Join("; ", normalized.Diagnostics.Select(static d => d.Message)));
            }

            var location = new ThemeSourceLocation("design-language-contract", 1, 1, "/Theme");
            var definition = new PreparedThemeDefinition("DesignLanguageContract", "Design Language Contract", _language,
                null, null, null, new HashSet<string>(StringComparer.Ordinal), ThemeSectionDefinition.Empty,
                Array.Empty<ThemeConditionDefinition>(), location);
            var result = new ThemeCompiler().Compile(new ThemeCompileInput(definition,
                new ThemeDefinitionRevision("design-language-contract", "1", "A1"), normalized.Config!, registry));
            if (!result.Success)
            {
                throw new InvalidOperationException(
                    $"Compiling '{_language.Id}' under {Text(variant)} failed; the contract cannot be verified: " +
                    string.Join("; ", result.Diagnostics.Select(static d => d.Message)), result.Exception);
            }

            var snapshot = result.Snapshot!;
            var controls = new Dictionary<ControlTokenIdentity, ControlThemeSnapshot>();
            foreach (var control in registry.Controls)
            {
                try
                {
                    controls.Add(control.Identity, snapshot.Controls[control.Slot]);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"Compiling control '{control.Identity}' under {Text(variant)} failed; the contract cannot be verified.",
                        exception);
                }
            }

            return new Compiled(snapshot, controls);
        }

        private void CompareCompiled(string check, DesignVariant variant, Compiled left, Compiled right)
        {
            foreach (var descriptor in Globals)
            {
                if (!SameValue(descriptor, left.Snapshot.GlobalTokenValues.GetValue(descriptor.Slot),
                        right.Snapshot.GlobalTokenValues.GetValue(descriptor.Slot)))
                {
                    ReportDifferent(check, variant, descriptor.Name);
                }
            }

            foreach (var descriptor in _language.LanguageTokens)
            {
                if (!SameValue(descriptor, left.Snapshot.LanguageTokenValues.GetValue(descriptor.Slot),
                        right.Snapshot.LanguageTokenValues.GetValue(descriptor.Slot)))
                {
                    ReportDifferent(check, variant, descriptor.Name);
                }
            }

            foreach (var control in registry.Controls)
            {
                var a = left.Controls[control.Identity];
                var b = right.Controls[control.Identity];
                foreach (var own in control.OwnTokens)
                {
                    if (!SameValue(own, a.ControlTokenValues.GetValue(own.Slot), b.ControlTokenValues.GetValue(own.Slot)))
                    {
                        ReportDifferent(check, variant, $"{control.Identity}.{own.Name}");
                    }
                }
            }
        }

        private void ReportDifferent(string check, DesignVariant variant, string tokenName) =>
            Report(check, variant, tokenName, check == Pure
                ? $"Compiling other variants in between changed the value of '{tokenName}'."
                : $"Two compilations with the same input produced different values for '{tokenName}'.");

        private void CheckReadable(DesignVariant variant, Compiled compiled)
        {
            var snapshot = compiled.Snapshot;
            var baseBackground = ColorOrNull(snapshot, nameof(DesignToken.ColorBgBase)) ??
                                 (variant.Appearance == ThemeAppearance.Dark ? Colors.Black : Colors.White);
            var container = ContrastRatio.Composite(ColorOf(snapshot, nameof(DesignToken.ColorBgContainer)), baseBackground);
            var primary = ContrastRatio.Composite(ColorOf(snapshot, nameof(DesignToken.ColorPrimary)), container);
            CheckContrast(variant, snapshot, nameof(DesignToken.ColorText), container, nameof(DesignToken.ColorBgContainer),
                ContrastRatio.BodyText);
            CheckContrast(variant, snapshot, nameof(DesignToken.ColorTextLightSolid), primary, nameof(DesignToken.ColorPrimary),
                ContrastRatio.TextOnSolid);
        }

        private void CheckContrast(DesignVariant variant, ThemeSnapshot snapshot, string foregroundName, Color background,
            string backgroundName, double threshold)
        {
            var foreground = ColorOf(snapshot, foregroundName);
            var ratio = ContrastRatio.Of(foreground, background);
            if (ratio < threshold)
            {
                Report(Readable, variant, foregroundName, string.Format(CultureInfo.InvariantCulture,
                    "Contrast of '{0}' {1} on '{2}' {3} is {4:F2}, below {5}.",
                    foregroundName, foreground, backgroundName, background, ratio, threshold));
            }
        }

        // ---------------------------------------------------------------- Helpers

        private Color ColorOf(ThemeSnapshot snapshot, string name) =>
            ColorOrNull(snapshot, name) ?? throw new InvalidOperationException($"Global token '{name}' has no value.");

        private Color? ColorOrNull(ThemeSnapshot snapshot, string name) =>
            registry.TryGetGlobalToken(name, out var descriptor) &&
            snapshot.GlobalTokenValues.GetValue(descriptor!.Slot) is Color color
                ? color
                : null;

        private static bool SameValue(TokenDescriptor descriptor, object? left, object? right)
        {
            if (Equals(left, right))
            {
                return true;
            }

            return left is not null && right is not null &&
                   string.Equals(descriptor.Format(left), descriptor.Format(right), StringComparison.Ordinal);
        }

        private static string Text(DesignVariant variant) =>
            $"{variant.Style}/{variant.Appearance}/{variant.Density}/{variant.Idiom}";

        private void Report(string check, DesignVariant variant, string? tokenName, string message) =>
            Report(check, Text(variant), tokenName, message);

        private void Report(string check, string variant, string? tokenName, string message)
        {
            var violation = new DesignLanguageContractViolation(check, variant, tokenName, message);
            if (!_violations.Contains(violation))
            {
                _violations.Add(violation);
            }
        }
    }

    /// <summary>
    /// Two distinct prefill values per value type, used by the completeness checks. A token of a type without samples
    /// fails the verification instead of being skipped.
    /// </summary>
    private static class TokenSamples
    {
        private static readonly Dictionary<Type, (object A, object B)> s_samples = new()
        {
            [typeof(Color)]        = (Color.FromArgb(0xFF, 0x12, 0x34, 0x56), Color.FromArgb(0x80, 0x65, 0x43, 0x21)),
            [typeof(double)]       = (1.25d, 2.75d),
            [typeof(float)]        = (1.25f, 2.75f),
            [typeof(int)]          = (3, 7),
            [typeof(bool)]         = (true, false),
            [typeof(string)]       = ("contract-a", "contract-b"),
            [typeof(CornerRadius)] = (new CornerRadius(1.25), new CornerRadius(2.75)),
            [typeof(Thickness)]    = (new Thickness(1.25), new Thickness(2.75)),
            [typeof(TimeSpan)]     = (TimeSpan.FromMilliseconds(125), TimeSpan.FromMilliseconds(275)),
            [typeof(FontFamily)]   = (new FontFamily("Contract A"), new FontFamily("Contract B")),
            [typeof(BoxShadows)]   = (new BoxShadows(new BoxShadow { Blur = 1.25 }), new BoxShadows(new BoxShadow { Blur = 2.75 })),
            [typeof(IBrush)]       = (new SolidColorBrush(Color.FromRgb(0x12, 0x34, 0x56)), new SolidColorBrush(Color.FromRgb(0x65, 0x43, 0x21))),
            [typeof(TextDecorationInfo)] = (new TextDecorationInfo { Thickness = 3 }, new TextDecorationInfo { Thickness = 7 }),
            [typeof(Dimension)]    = (new Dimension(1.25), new Dimension(2.75)),
            [typeof(Size)]         = (new Size(1.25, 1.25), new Size(2.75, 2.75)),
            [typeof(Point)]        = (new Point(1.25, 1.25), new Point(2.75, 2.75)),
            [typeof(BoxShadow)]    = (new BoxShadow { Blur = 1.25 }, new BoxShadow { Blur = 2.75 }),
            [typeof(SolidColorBrush)] = (new SolidColorBrush(Color.FromRgb(0x12, 0x34, 0x56)), new SolidColorBrush(Color.FromRgb(0x65, 0x43, 0x21))),
            [typeof(IImmutableBrush)] = (new Avalonia.Media.Immutable.ImmutableSolidColorBrush(Color.FromRgb(0x12, 0x34, 0x56)),
                new Avalonia.Media.Immutable.ImmutableSolidColorBrush(Color.FromRgb(0x65, 0x43, 0x21))),
            [typeof(Avalonia.Media.Immutable.ImmutableTransform)] = (
                new Avalonia.Media.Immutable.ImmutableTransform(Matrix.CreateTranslation(1, 2)),
                new Avalonia.Media.Immutable.ImmutableTransform(Matrix.CreateTranslation(3, 4))),
            [typeof(Avalonia.Animation.Easings.Easing)] = (new Avalonia.Animation.Easings.LinearEasing(),
                new Avalonia.Animation.Easings.CubicEaseIn()),
        };

        internal static (object A, object B) Get(TokenDescriptor descriptor)
        {
            var type = Nullable.GetUnderlyingType(descriptor.ValueType) ?? descriptor.ValueType;
            if (s_samples.TryGetValue(type, out var samples))
            {
                return samples;
            }

            if (type.IsEnum && Enum.GetValues(type) is { Length: >= 2 } values)
            {
                return (values.GetValue(0)!, values.GetValue(1)!);
            }

            throw new InvalidOperationException(
                $"No contract samples for value type '{descriptor.ValueType}' of token '{descriptor.Name}'. " +
                "Add two distinct samples for it to DesignLanguageContract.TokenSamples.");
        }
    }
}
