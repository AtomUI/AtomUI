using System.Globalization;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeConfigNormalizerTests
{
    [Fact]
    public void Normalize_Captures_Typed_Values_Axes_And_Classifies_Control_Tokens()
    {
        var schema = ThemeConfigTestSchema.Create();
        var buttonConfig = new ControlThemeConfigBuilder()
                           .WithDensity(DesignDensity.Compact)
                           .WithAppearance(ThemeAppearance.Dark)
                           .WithToken("Alpha", "2.5")
                           .WithToken("Height", "32")
                           .Build();
        var input = new ThemeConfigBuilder()
                    .WithLanguage("Probe")
                    .WithDensity(DesignDensity.Compact)
                    .WithToken("Alpha", "1.5")
                    .WithControl(ThemeConfigTestSchema.ButtonIdentity, buttonConfig)
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, schema);

        result.Success.ShouldBeTrue();
        var normalized = result.Config.ShouldNotBeNull();
        normalized.Language.ShouldNotBeNull().Id.ShouldBe("Probe");
        normalized.Axes.Density.ShouldBe(DesignDensity.Compact);
        normalized.Axes.Appearance.ShouldBeNull();
        normalized.GlobalTokens.Single().Value.ShouldBe(1.5d);
        var button = normalized.Controls.Single();
        button.Density.ShouldBe(DesignDensity.Compact);
        button.Appearance.ShouldBe(ThemeAppearance.Dark);
        button.GlobalTokens.Single().Value.ShouldBe(2.5d);
        button.OwnTokens.ShouldHaveSingleItem().Value.ShouldBe(32d);
    }

    [Fact]
    public void Normalize_Accepts_Any_Registered_Global_Token_For_The_Control()
    {
        var input = new ThemeConfigBuilder()
                    .WithControl(
                        ThemeConfigTestSchema.ButtonIdentity,
                        new ControlThemeConfigBuilder()
                            .WithToken("Beta", "2")
                            .Build())
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, ThemeConfigTestSchema.Create());

        result.Success.ShouldBeTrue();
        result.Diagnostics.ShouldBeEmpty();
        var control = result.Config.ShouldNotBeNull().Controls.ShouldHaveSingleItem();
        control.GlobalTokens.ShouldHaveSingleItem().Descriptor.Name.ShouldBe("Beta");
        control.OwnTokens.ShouldBeEmpty();
    }

    [Fact]
    public void Normalize_Produces_Order_And_Culture_Independent_Fingerprint()
    {
        var schema = ThemeConfigTestSchema.Create();
        var first = new ThemeConfigBuilder()
                    .WithLanguage("Probe")
                    .WithDensity(DesignDensity.Compact)
                    .WithToken("Beta", "2.50")
                    .WithToken("Alpha", "1.50")
                    .Build();
        var second = new ThemeConfigBuilder()
                     .WithDensity(DesignDensity.Compact)
                     .WithLanguage("Probe")
                     .WithToken("Alpha", "1.5")
                     .WithToken("Beta", "2.5")
                     .Build();

        NormalizedThemeConfig firstNormalized;
        using (new CultureScope("fr-FR"))
        {
            firstNormalized = ThemeConfigNormalizer.Normalize(first, schema).Config.ShouldNotBeNull();
        }

        var secondNormalized = ThemeConfigNormalizer.Normalize(second, schema).Config.ShouldNotBeNull();
        firstNormalized.ShouldBe(secondNormalized);
        firstNormalized.Fingerprint.ShouldBe(secondNormalized.Fingerprint);
    }

    [Fact]
    public void Normalize_Leaves_Unwritten_Axes_To_The_Lower_Layer()
    {
        var result = ThemeConfigNormalizer.Normalize(new ThemeConfigBuilder().Build(), ThemeConfigTestSchema.Create());

        result.Success.ShouldBeTrue();
        result.Config.ShouldNotBeNull().Axes.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void Normalize_Rejects_A_Language_That_Is_Not_Enabled()
    {
        var input = new ThemeConfigBuilder()
                    .WithLanguage("Unregistered")
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, ThemeConfigTestSchema.Create());

        result.Success.ShouldBeFalse();
        result.Config.ShouldBeNull();
        result.Diagnostics.ShouldContain(static diagnostic =>
            diagnostic.Code == "ATMTHM4004" &&
            diagnostic.Path == "$.Language");
    }

    [Theory]
    [InlineData("$.Style")]
    [InlineData("$.Density")]
    [InlineData("$.Idiom")]
    public void Normalize_Rejects_An_Axis_Value_The_Language_Does_Not_Declare(string path)
    {
        var builder = new ThemeConfigBuilder().WithLanguage("Probe");
        _ = path switch
        {
            "$.Style" => builder.WithStyle(new DesignStyle("Undeclared")),
            "$.Density" => builder.WithDensity(new DesignDensity("Spacious")),
            _ => builder.WithIdiom(DesignIdiom.Mobile)
        };

        var result = ThemeConfigNormalizer.Normalize(builder.Build(), ThemeConfigTestSchema.Create());

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(diagnostic => diagnostic.Code == "ATMTHM4005" && diagnostic.Path == path);
    }

    [Fact]
    public void Normalize_Rejects_A_Control_Density_The_Language_Does_Not_Declare()
    {
        var input = new ThemeConfigBuilder()
                    .WithLanguage("Probe")
                    .WithControl(
                        ThemeConfigTestSchema.ButtonIdentity,
                        new ControlThemeConfigBuilder()
                            .WithDensity(new DesignDensity("Spacious"))
                            .Build())
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, ThemeConfigTestSchema.Create());

        result.Success.ShouldBeFalse();
        result.Config.ShouldBeNull();
        result.Diagnostics.ShouldContain(static diagnostic =>
            diagnostic.Code == "ATMTHM4005" &&
            diagnostic.Path == "$.Controls['AtomUI:Button'].Density");
    }

    [Fact]
    public void Normalize_Returns_Diagnostics_Without_Publishing_Partial_Config()
    {
        var schema = ThemeConfigTestSchema.Create();
        var input = new ThemeConfigBuilder()
                    .WithLanguage("Unregistered")
                    .WithToken("Alpha", "not-a-number")
                    .WithToken("Missing", "1")
                    .WithControl(
                        new ControlTokenIdentity("AtomUI", "Missing"),
                        new ControlThemeConfigBuilder().Build())
                    .WithControl(
                        ThemeConfigTestSchema.ButtonIdentity,
                        new ControlThemeConfigBuilder()
                            .WithAppearance((ThemeAppearance)42)
                            .Build())
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, schema);

        result.Success.ShouldBeFalse();
        result.Config.ShouldBeNull();
        result.Diagnostics.Select(static item => item.Code).ShouldContain("ATMTHM4001");
        result.Diagnostics.Select(static item => item.Code).ShouldContain("ATMTHM4002");
        result.Diagnostics.Select(static item => item.Code).ShouldContain("ATMTHM4004");
        result.Diagnostics.Select(static item => item.Code).ShouldContain("ATMTHM4005");
        result.Diagnostics.Select(static item => item.Code).ShouldContain("ATMTHM4006");
    }

    [Fact]
    public void Config_Fingerprint_Is_Deterministic_Across_Processes()
    {
        // No language: a language's revision is its assembly's module version, which changes with every build.
        var config = new ThemeConfigBuilder()
                     .WithDensity(DesignDensity.Compact)
                     .WithAppearance(ThemeAppearance.Dark)
                     .WithToken("Alpha", "1.5")
                     .Build();

        var first = ThemeConfigNormalizer.Normalize(config, ThemeConfigTestSchema.Create()).Config.ShouldNotBeNull();
        var second = ThemeConfigNormalizer.Normalize(config, ThemeConfigTestSchema.Create()).Config.ShouldNotBeNull();

        second.Fingerprint.ShouldBe(first.Fingerprint);
        // Pins cross-process stability: the fingerprint must never depend on string.GetHashCode().
        first.Fingerprint.Value.ShouldBe(17367421940583163063UL);
    }

    [Fact]
    public void Normalize_Rejects_A_Parser_Result_That_Does_Not_Match_The_Token_Value_Type()
    {
        var badToken = new TokenDescriptor(
            "Alpha",
            0,
            TokenStage.Seed,
            typeof(double),
            "Alpha",
            static _ => "not-a-double",
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static token => ((BadDesignToken)token).Value,
            static (token, value) => ((BadDesignToken)token).Value = (double)value!,
            static token => ((BadDesignToken)token).Value);
        var registry = new ThemeSchemaRegistry(
            [badToken],
            Array.Empty<ControlTokenDescriptor>(),
            Array.Empty<DesignLanguageDescriptor>());
        var input = new ThemeConfigBuilder()
                    .WithToken("Alpha", "1")
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, registry);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(static diagnostic =>
            diagnostic.Code == "ATMTHM4006" &&
            diagnostic.Path == "$.Tokens['Alpha']");
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _originalUICulture = CultureInfo.CurrentUICulture;

        internal CultureScope(string name)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(name);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUICulture;
        }
    }

    private sealed class BadDesignToken : AbstractDesignToken
    {
        internal double Value { get; set; }
    }
}

/// <summary>
/// A two-token schema (seed <c>Alpha</c>, alias <c>Beta</c>), the probe language and a button with one own token
/// (<c>Height</c>) and its probe rule.
/// </summary>
internal static class ThemeConfigTestSchema
{
    internal static readonly ControlTokenIdentity ButtonIdentity = TestControlToken.ControlIdentity;

    internal static ThemeSchemaRegistry Create()
    {
        var globalTokens = new[]
        {
            Token("Alpha", 0, TokenStage.Seed),
            Token("Beta", 1, TokenStage.Alias)
        };
        var ownTokens = new[]
        {
            Token("Height", 0, TokenStage.Control)
        };
        var control = new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", "Button"),
            ButtonIdentity,
            ownTokens,
            static () => new TestControlToken());
        return new ThemeSchemaRegistry(globalTokens, [control], [TestDesignLanguages.Probe()], null,
            [ControlTokenRuleDescriptor.Create<ProbeLanguageToken, TestControlToken, TestControlTokenRule>()]);
    }

    private static TokenDescriptor Token(string name, int slot, TokenStage stage)
    {
        return new TokenDescriptor(
            name,
            slot,
            stage,
            typeof(double),
            name,
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static token => ((TestDesignToken)token).Value,
            static (token, value) => ((TestDesignToken)token).Value = (double)value!,
            static token => ((TestDesignToken)token).Value);
    }

    private sealed class TestDesignToken : AbstractDesignToken
    {
        internal double Value { get; set; }
    }

    internal sealed class TestControlToken : AbstractControlDesignToken, IControlToken
    {
        public static ControlTokenIdentity ControlIdentity { get; } = new("AtomUI", "Button");
    }

    internal sealed class TestControlTokenRule : IControlTokenRule<ProbeLanguageToken, TestControlToken>
    {
        public void Apply(in TokenEvaluationContext<ProbeLanguageToken> context, ControlTokenTarget<TestControlToken> target)
        {
        }
    }
}
