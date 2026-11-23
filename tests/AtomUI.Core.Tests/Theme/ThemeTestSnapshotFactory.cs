using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Generated.AtomUICore;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Shouldly;

namespace AtomUI.Core.Tests.Theme;

internal static class ThemeTestSnapshotFactory
{
    /// <summary>
    /// Compiles <paramref name="config"/> over the Ant Design language default with the given controls; the test
    /// controls of this project bring their own derivations (<see cref="TestControlDerivations"/>).
    /// </summary>
    internal static ThemeSnapshot Compile(
        ThemeConfig? config = null,
        params ControlTokenDescriptor[] controls)
    {
        var registry = CreateRegistry(controls);
        var definition = CreateLanguageDefinition(registry.Languages[0]);
        var normalized = ThemeConfigNormalizer.Normalize(config ?? new ThemeConfigBuilder().Build(), registry);
        normalized.Success.ShouldBeTrue();
        var defaults = ThemeCompiler.CreateDefinitionDefaults(definition, registry);
        var effective = ThemeConfigMerger.Merge(defaults, null, normalized.Config!).EffectiveConfig;
        var result = new ThemeCompiler().Compile(new ThemeCompileInput(
            definition,
            new ThemeDefinitionRevision("test", "1", effective.Fingerprint.ToString()),
            effective,
            registry));

        result.Exception.ShouldBeNull();
        result.Success.ShouldBeTrue();
        return result.Snapshot!;
    }

    /// <summary>A registry with the generated global tokens, Ant Design, the controls and the test derivations.</summary>
    internal static ThemeSchemaRegistry CreateRegistry(
        IReadOnlyList<ControlTokenDescriptor> controls,
        IEnumerable<ControlDerivationDescriptor>? derivations = null,
        DesignLanguageDescriptor? language = null) =>
        new(GeneratedThemeSchema.GetGlobalTokens(), controls, [language ?? TestDesignLanguages.AntDesign], null,
            derivations ?? TestControlDerivations.All);

    /// <summary>
    /// The implicit default entry of a language: no values, no fixed axes. One instance per language and id, as the
    /// catalog keeps one, so that compilations can reuse a previous snapshot's layers.
    /// </summary>
    internal static PreparedThemeDefinition CreateLanguageDefinition(DesignLanguageDescriptor language, string id = "TestTheme") =>
        s_definitions.GetValue(language, static _ => new System.Collections.Concurrent.ConcurrentDictionary<string, PreparedThemeDefinition>())
                     .GetOrAdd(id, static (key, value) => new PreparedThemeDefinition(key, "Test Theme", value, null, null,
                         null, new HashSet<string>(StringComparer.Ordinal), ThemeSectionDefinition.Empty,
                         Array.Empty<ThemeConditionDefinition>(), new ThemeSourceLocation("test", 1, 1, "/Theme")), language);

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<DesignLanguageDescriptor,
        System.Collections.Concurrent.ConcurrentDictionary<string, PreparedThemeDefinition>> s_definitions = new();

    internal static T Global<T>(this ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var descriptor).ShouldBeTrue();
        return snapshot.GlobalTokenValues.Get<T>(descriptor!.Slot);
    }

    internal static T EffectiveGlobal<T>(
        this ThemeSnapshot snapshot,
        ControlTokenIdentity identity,
        string name)
    {
        snapshot.Registry.TryGetControl(identity, out var control).ShouldBeTrue();
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue();
        return snapshot.Controls[control!.Slot]
                       .GetEffectiveGlobalValue<T>(snapshot.GlobalTokenValues, token!.Slot);
    }

    internal static T Control<T>(
        this ThemeSnapshot snapshot,
        ControlTokenIdentity identity,
        int tokenSlot)
    {
        snapshot.Registry.TryGetControl(identity, out var control).ShouldBeTrue();
        return snapshot.Controls[control!.Slot].ControlTokenValues.Get<T>(tokenSlot);
    }
}

/// <summary>The Ant Design derivations of the hand-written test controls of this project.</summary>
internal static class TestControlDerivations
{
    internal static IReadOnlyList<ControlDerivationDescriptor> All { get; } =
    [
        ControlDerivationDescriptor.Create<AntDesignToken, CompilerButtonToken, CompilerButtonDerivation>(),
        ControlDerivationDescriptor.Create<AntDesignToken, BrushControlToken, BrushControlDerivation>(),
        ControlDerivationDescriptor.Create<AntDesignToken, OwnerTokenResourceTests.ThrowingLineEditToken,
            OwnerTokenResourceTests.ThrowingLineEditDerivation>()
    ];
}

internal static class ThemeCompilerTests
{
    internal static ControlTokenDescriptor CreateCompilerButtonDescriptor()
    {
        var height = new TokenDescriptor(
            nameof(CompilerButtonToken.Height),
            0,
            TokenStage.Control,
            typeof(double),
            CompilerButtonTokenKind.Height,
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static token => ((CompilerButtonToken)token).Height,
            static (token, value) => ((CompilerButtonToken)token).Height = (double)value!,
            static token => ((CompilerButtonToken)token).Height);
        return new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", CompilerButtonToken.ID),
            CompilerButtonToken.ControlIdentity,
            [height],
            static () => new CompilerButtonToken());
    }
}

internal enum CompilerButtonTokenKind
{
    Height
}

/// <summary>A control token written by hand the way the generator writes one, with the freeze check.</summary>
internal sealed class CompilerButtonToken : AbstractControlDesignToken, IControlToken
{
    internal const string ID = "Button";

    public static ControlTokenIdentity ControlIdentity { get; } = new("AtomUI", ID);

    public double Height
    {
        get => field;
        set
        {
            if (IsFrozen(nameof(Height)))
            {
                return;
            }
            field = value;
        }
    }
}

/// <summary>The test button's height follows the global control height.</summary>
internal sealed class CompilerButtonDerivation : IControlDerivation<AntDesignToken, CompilerButtonToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<CompilerButtonToken> view) =>
        view.Token.Height = view.Global.ControlHeight;
}
