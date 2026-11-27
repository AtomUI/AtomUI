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
/// Registration of design languages and control token rules: the lookup walks the language token's base
/// types, the nearest registration wins, and duplicates are rejected.
/// </summary>
public class DesignLanguageRegistryTests
{
    [Fact]
    public void A_Derived_Language_Finds_The_Token_Rule_Of_Its_Base_Language_Token()
    {
        var grandchild = Language<GrandchildProbeToken>("Grandchild");
        var registry = Registry([grandchild], [CreateRule<ProbeLanguageToken, BaseTokenRule>()]);

        Find(registry, grandchild).ShouldBeSameAs(registry.ControlTokenRules.Single());
    }

    [Fact]
    public void The_Nearest_Registration_Along_The_Chain_Wins()
    {
        var child = Language<ChildProbeToken>("Child");
        var grandchild = Language<GrandchildProbeToken>("Grandchild");
        var probe = TestDesignLanguages.Probe();
        var baseRule = CreateRule<ProbeLanguageToken, BaseTokenRule>();
        var childRule = CreateRule<ChildProbeToken, ChildTokenRule>();
        var registry = Registry([probe, child, grandchild], [baseRule, childRule]);

        Find(registry, probe).ShouldBeSameAs(baseRule);
        Find(registry, child).ShouldBeSameAs(childRule);
        Find(registry, grandchild).ShouldBeSameAs(childRule);
    }

    [Fact]
    public void An_Exact_Registration_Wins_Over_Every_Base_Registration()
    {
        var grandchild = Language<GrandchildProbeToken>("Grandchild");
        var exact = CreateRule<GrandchildProbeToken, GrandchildTokenRule>();
        var registry = Registry([grandchild],
            [CreateRule<ProbeLanguageToken, BaseTokenRule>(), CreateRule<ChildProbeToken, ChildTokenRule>(), exact]);

        Find(registry, grandchild).ShouldBeSameAs(exact);
    }

    [Fact]
    public void Two_Token_Rules_For_The_Same_Language_Token_And_Control_Are_Rejected()
    {
        var exception = Should.Throw<ThemeSchemaException>(() => Registry([TestDesignLanguages.Probe()],
            [CreateRule<ProbeLanguageToken, BaseTokenRule>(), CreateRule<ProbeLanguageToken, OtherBaseTokenRule>()]));

        exception.Message.ShouldContain("Duplicate control token rule");
        exception.Message.ShouldContain(ViewProbeToken.ControlIdentity.ToString());
    }

    [Fact]
    public void Two_Languages_With_The_Same_Id_Are_Rejected()
    {
        var exception = Should.Throw<ThemeSchemaException>(() => Registry(
            [TestDesignLanguages.Probe(), TestDesignLanguages.Probe()], [CreateRule<ProbeLanguageToken, BaseTokenRule>()]));

        exception.Message.ShouldContain("'Probe'");
    }

    [Fact]
    public void A_Token_Rule_For_A_Control_That_Is_Not_Enabled_Stays_Dormant()
    {
        var registry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), [], [TestDesignLanguages.Probe()],
            null, [CreateRule<ProbeLanguageToken, BaseTokenRule>()]);

        registry.ControlTokenRules.Count.ShouldBe(1);
        registry.Controls.Count.ShouldBe(0);
    }

    [Fact]
    public void The_Builder_Keeps_One_Registration_Per_Descriptor()
    {
        var builder = new ThemeManagerBuilder();
        var language = TestDesignLanguages.Probe();
        var rule = CreateRule<ProbeLanguageToken, BaseTokenRule>();

        builder.AddDesignLanguage(language).AddDesignLanguage(language);
        builder.AddControlTokenRules([rule]).AddControlTokenRules([rule]);

        builder.Languages.ShouldBe([language]);
        builder.TokenRules.ShouldBe([rule]);
    }

    [Fact]
    public void The_Language_Revision_Is_The_Module_Version_Of_Its_Assembly()
    {
        var language = TestDesignLanguages.Probe();
        var rule = CreateRule<ProbeLanguageToken, BaseTokenRule>();

        language.Revision.ShouldBe(typeof(ProbeLanguage).Assembly.ManifestModule.ModuleVersionId);
        rule.Revision.ShouldBe(typeof(BaseTokenRule).Assembly.ManifestModule.ModuleVersionId);
    }

    [Fact]
    public void An_Append_Keeps_The_Committed_Token_Rules()
    {
        var probe = TestDesignLanguages.Probe();
        var rule = CreateRule<ProbeLanguageToken, BaseTokenRule>();
        var committed = Registry([probe], [rule]);

        Registry([probe], [rule]).ValidateAppendedTokenRules(committed);
    }

    [Fact]
    public void An_Append_Cannot_Change_The_Token_Rule_Of_A_Committed_Control_Under_A_Committed_Language()
    {
        // The committed probe language finds the child rule's base; a new rule that a committed language
        // would find for a committed control would change committed values.
        var child = Language<ChildProbeToken>("Child");
        var committed = Registry([child], [CreateRule<ProbeLanguageToken, BaseTokenRule>()]);
        var appended = Registry([child],
            [CreateRule<ProbeLanguageToken, BaseTokenRule>(), CreateRule<ChildProbeToken, ChildTokenRule>()]);

        var exception = Should.Throw<ThemeLoadException>(() => appended.ValidateAppendedTokenRules(committed));
        exception.Message.ShouldContain(ViewProbeToken.ControlIdentity.ToString());
    }

    [Fact]
    public void An_Append_May_Bring_A_Token_Rule_For_A_New_Language()
    {
        var probe = TestDesignLanguages.Probe();
        var committed = Registry([probe], [CreateRule<ProbeLanguageToken, BaseTokenRule>()]);
        var grandchild = Language<GrandchildProbeToken>("Grandchild");
        var appended = Registry([probe, grandchild],
            [committed.ControlTokenRules.Single(), CreateRule<GrandchildProbeToken, GrandchildTokenRule>()]);

        appended.ValidateAppendedTokenRules(committed);
    }

    [Fact]
    public void An_Append_May_Bring_A_Token_Rule_For_A_New_Control()
    {
        var probe = TestDesignLanguages.Probe();
        var committed = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), [], [probe], null, []);
        var appended = Registry([probe], [CreateRule<ProbeLanguageToken, BaseTokenRule>()]);

        appended.ValidateAppendedTokenRules(committed);
    }

    // ---------------------------------------------------------------- Helpers

    private static ThemeSchemaRegistry Registry(
        IReadOnlyList<DesignLanguageDescriptor> languages,
        IReadOnlyList<ControlTokenRuleDescriptor> rules) =>
        new(GeneratedThemeSchema.GetGlobalTokens(), [ViewProbeToken.CreateDescriptor()], languages, null, rules);

    private static ControlTokenRuleDescriptor? Find(ThemeSchemaRegistry registry, DesignLanguageDescriptor language)
    {
        registry.TryGetControl(ViewProbeToken.ControlIdentity, out var control).ShouldBeTrue();
        return registry.FindControlTokenRule(language, control!.Slot);
    }

    private static ControlTokenRuleDescriptor CreateRule<TLanguageToken, TRule>()
        where TLanguageToken : LanguageDesignToken
        where TRule : class, IControlTokenRule<TLanguageToken, ViewProbeToken>, new() =>
        ControlTokenRuleDescriptor.Create<TLanguageToken, ViewProbeToken, TRule>();

    private static DesignLanguageDescriptor Language<TToken>(string id) where TToken : LanguageDesignToken, new() =>
        new(new ChainLanguage<TToken>(id), Array.Empty<TokenDescriptor>());

    private sealed class BaseTokenRule : IControlTokenRule<ProbeLanguageToken, ViewProbeToken>
    {
        public void Apply(in TokenEvaluationContext<ProbeLanguageToken> context, ControlTokenTarget<ViewProbeToken> target) =>
            target.Own.Accent = Colors.Red;
    }

    private sealed class OtherBaseTokenRule : IControlTokenRule<ProbeLanguageToken, ViewProbeToken>
    {
        public void Apply(in TokenEvaluationContext<ProbeLanguageToken> context, ControlTokenTarget<ViewProbeToken> target) =>
            target.Own.Accent = Colors.Yellow;
    }

    private sealed class ChildTokenRule : IControlTokenRule<ChildProbeToken, ViewProbeToken>
    {
        public void Apply(in TokenEvaluationContext<ChildProbeToken> context, ControlTokenTarget<ViewProbeToken> target) =>
            target.Own.Accent = Colors.Green;
    }

    private sealed class GrandchildTokenRule : IControlTokenRule<GrandchildProbeToken, ViewProbeToken>
    {
        public void Apply(in TokenEvaluationContext<GrandchildProbeToken> context, ControlTokenTarget<ViewProbeToken> target) =>
            target.Own.Accent = Colors.Blue;
    }
}

/// <summary>
/// A language token derived from <see cref="ProbeLanguageToken" />: a language built on the probe language.
/// </summary>
public class ChildProbeToken : ProbeLanguageToken;

/// <summary>
/// A second level of rule.
/// </summary>
public sealed class GrandchildProbeToken : ChildProbeToken;

/// <summary>
/// A language with no rule of its own, used to test the lookup along its token's base types.
/// </summary>
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

    public override void DeriveGlobal(in TokenEvaluationContext<TToken> context, DesignToken token)
    {
    }
}
