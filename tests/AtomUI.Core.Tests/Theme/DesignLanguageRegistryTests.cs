using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// Registration of design languages and control derivations (spec 6.1, 8): the lookup walks the language token's base
/// types, the nearest registration wins, and duplicates are rejected.
/// </summary>
public class DesignLanguageRegistryTests
{
    [Fact]
    public void A_Derived_Language_Finds_The_Derivation_Of_Its_Base_Language_Token()
    {
        var grandchild = Language<GrandchildProbeToken>("Grandchild");
        var registry = Registry([grandchild], [Derivation<ProbeLanguageToken, BaseDerivation>()]);

        Find(registry, grandchild).ShouldBeSameAs(registry.ControlDerivations.Single());
    }

    [Fact]
    public void The_Nearest_Registration_Along_The_Chain_Wins()
    {
        var child = Language<ChildProbeToken>("Child");
        var grandchild = Language<GrandchildProbeToken>("Grandchild");
        var probe = TestDesignLanguages.Probe();
        var baseDerivation = Derivation<ProbeLanguageToken, BaseDerivation>();
        var childDerivation = Derivation<ChildProbeToken, ChildDerivation>();
        var registry = Registry([probe, child, grandchild], [baseDerivation, childDerivation]);

        Find(registry, probe).ShouldBeSameAs(baseDerivation);
        Find(registry, child).ShouldBeSameAs(childDerivation);
        Find(registry, grandchild).ShouldBeSameAs(childDerivation);
    }

    [Fact]
    public void An_Exact_Registration_Wins_Over_Every_Base_Registration()
    {
        var grandchild = Language<GrandchildProbeToken>("Grandchild");
        var exact = Derivation<GrandchildProbeToken, GrandchildDerivation>();
        var registry = Registry([grandchild],
            [Derivation<ProbeLanguageToken, BaseDerivation>(), Derivation<ChildProbeToken, ChildDerivation>(), exact]);

        Find(registry, grandchild).ShouldBeSameAs(exact);
    }

    [Fact]
    public void Two_Derivations_For_The_Same_Language_Token_And_Control_Are_Rejected()
    {
        var exception = Should.Throw<ThemeSchemaException>(() => Registry([TestDesignLanguages.Probe()],
            [Derivation<ProbeLanguageToken, BaseDerivation>(), Derivation<ProbeLanguageToken, OtherBaseDerivation>()]));

        exception.Message.ShouldContain("Duplicate control derivation");
        exception.Message.ShouldContain(ViewProbeToken.ControlIdentity.ToString());
    }

    [Fact]
    public void Two_Languages_With_The_Same_Id_Are_Rejected()
    {
        var exception = Should.Throw<ThemeSchemaException>(() => Registry(
            [TestDesignLanguages.Probe(), TestDesignLanguages.Probe()], [Derivation<ProbeLanguageToken, BaseDerivation>()]));

        exception.Message.ShouldContain("'Probe'");
    }

    [Fact]
    public void A_Derivation_For_A_Control_That_Is_Not_Enabled_Stays_Dormant()
    {
        var registry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), [], [TestDesignLanguages.Probe()],
            null, [Derivation<ProbeLanguageToken, BaseDerivation>()]);

        registry.ControlDerivations.Count.ShouldBe(1);
        registry.Controls.Count.ShouldBe(0);
    }

    [Fact]
    public void The_Builder_Keeps_One_Registration_Per_Descriptor()
    {
        var builder = new ThemeManagerBuilder();
        var language = TestDesignLanguages.Probe();
        var derivation = Derivation<ProbeLanguageToken, BaseDerivation>();

        builder.AddDesignLanguage(language).AddDesignLanguage(language);
        builder.AddControlDerivations([derivation]).AddControlDerivations([derivation]);

        builder.Languages.ShouldBe([language]);
        builder.Derivations.ShouldBe([derivation]);
    }

    [Fact]
    public void The_Language_Revision_Is_The_Module_Version_Of_Its_Assembly()
    {
        var language = TestDesignLanguages.Probe();
        var derivation = Derivation<ProbeLanguageToken, BaseDerivation>();

        language.Revision.ShouldBe(typeof(ProbeLanguage).Assembly.ManifestModule.ModuleVersionId);
        derivation.Revision.ShouldBe(typeof(BaseDerivation).Assembly.ManifestModule.ModuleVersionId);
    }

    [Fact]
    public void An_Append_Keeps_The_Committed_Derivations()
    {
        var probe = TestDesignLanguages.Probe();
        var derivation = Derivation<ProbeLanguageToken, BaseDerivation>();
        var committed = Registry([probe], [derivation]);

        Registry([probe], [derivation]).ValidateAppendedDerivations(committed);
    }

    [Fact]
    public void An_Append_Cannot_Change_The_Derivation_Of_A_Committed_Control_Under_A_Committed_Language()
    {
        // The committed probe language finds the child derivation's base; a new derivation that a committed language
        // would find for a committed control would change committed values.
        var child = Language<ChildProbeToken>("Child");
        var committed = Registry([child], [Derivation<ProbeLanguageToken, BaseDerivation>()]);
        var appended = Registry([child],
            [Derivation<ProbeLanguageToken, BaseDerivation>(), Derivation<ChildProbeToken, ChildDerivation>()]);

        var exception = Should.Throw<ThemeLoadException>(() => appended.ValidateAppendedDerivations(committed));
        exception.Message.ShouldContain(ViewProbeToken.ControlIdentity.ToString());
    }

    [Fact]
    public void An_Append_May_Bring_A_Derivation_For_A_New_Language()
    {
        var probe = TestDesignLanguages.Probe();
        var committed = Registry([probe], [Derivation<ProbeLanguageToken, BaseDerivation>()]);
        var grandchild = Language<GrandchildProbeToken>("Grandchild");
        var appended = Registry([probe, grandchild],
            [committed.ControlDerivations.Single(), Derivation<GrandchildProbeToken, GrandchildDerivation>()]);

        appended.ValidateAppendedDerivations(committed);
    }

    [Fact]
    public void An_Append_May_Bring_A_Derivation_For_A_New_Control()
    {
        var probe = TestDesignLanguages.Probe();
        var committed = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), [], [probe], null, []);
        var appended = Registry([probe], [Derivation<ProbeLanguageToken, BaseDerivation>()]);

        appended.ValidateAppendedDerivations(committed);
    }

    // ---------------------------------------------------------------- Helpers

    private static ThemeSchemaRegistry Registry(
        IReadOnlyList<DesignLanguageDescriptor> languages,
        IReadOnlyList<ControlDerivationDescriptor> derivations) =>
        new(GeneratedThemeSchema.GetGlobalTokens(), [ViewProbeToken.CreateDescriptor()], languages, null, derivations);

    private static ControlDerivationDescriptor? Find(ThemeSchemaRegistry registry, DesignLanguageDescriptor language)
    {
        registry.TryGetControl(ViewProbeToken.ControlIdentity, out var control).ShouldBeTrue();
        return registry.FindDerivation(language, control!.Slot);
    }

    private static ControlDerivationDescriptor Derivation<TLanguageToken, TDerivation>()
        where TLanguageToken : LanguageDesignToken
        where TDerivation : class, IControlDerivation<TLanguageToken, ViewProbeToken>, new() =>
        ControlDerivationDescriptor.Create<TLanguageToken, ViewProbeToken, TDerivation>();

    private static DesignLanguageDescriptor Language<TToken>(string id) where TToken : LanguageDesignToken, new() =>
        new(new ChainLanguage<TToken>(id), Array.Empty<TokenDescriptor>());

    private sealed class BaseDerivation : IControlDerivation<ProbeLanguageToken, ViewProbeToken>
    {
        public void Derive(in DerivationContext<ProbeLanguageToken> context, ControlView<ViewProbeToken> view) =>
            view.Token.Accent = Colors.Red;
    }

    private sealed class OtherBaseDerivation : IControlDerivation<ProbeLanguageToken, ViewProbeToken>
    {
        public void Derive(in DerivationContext<ProbeLanguageToken> context, ControlView<ViewProbeToken> view) =>
            view.Token.Accent = Colors.Yellow;
    }

    private sealed class ChildDerivation : IControlDerivation<ChildProbeToken, ViewProbeToken>
    {
        public void Derive(in DerivationContext<ChildProbeToken> context, ControlView<ViewProbeToken> view) =>
            view.Token.Accent = Colors.Green;
    }

    private sealed class GrandchildDerivation : IControlDerivation<GrandchildProbeToken, ViewProbeToken>
    {
        public void Derive(in DerivationContext<GrandchildProbeToken> context, ControlView<ViewProbeToken> view) =>
            view.Token.Accent = Colors.Blue;
    }
}

/// <summary>A language token derived from <see cref="ProbeLanguageToken"/>: a language built on the probe language.</summary>
public class ChildProbeToken : ProbeLanguageToken;

/// <summary>A second level of derivation.</summary>
public sealed class GrandchildProbeToken : ChildProbeToken;

/// <summary>A language with no derivation of its own, used to test the lookup along its token's base types.</summary>
public sealed class ChainLanguage<TToken>(string id) : DesignLanguage<TToken> where TToken : LanguageDesignToken, new()
{
    private static readonly DesignStyle s_style = new("Default");

    public override string Id => id;

    public override DesignLanguageAxes Axes { get; } =
        new([s_style], s_style, [DesignDensity.Default], [DesignIdiom.Desktop], []);

    public override void DefineSeeds(in DesignVariant variant, DesignToken seed, TToken languageSeed)
    {
    }

    public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, TToken language)
    {
    }

    public override void DeriveGlobal(in DerivationContext<TToken> context, DesignToken token)
    {
    }
}
