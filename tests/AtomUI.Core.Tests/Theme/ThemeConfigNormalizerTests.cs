using System.Globalization;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeConfigNormalizerTests
{
    [Fact]
    public void Normalize_Captures_Typed_Values_And_Classifies_Control_Tokens()
    {
        var schema = ThemeConfigTestSchema.Create();
        var buttonConfig = new ControlThemeConfigBuilder()
                           .WithAlgorithm(ControlAlgorithmMode.Global)
                           .WithToken("Alpha", "2.5")
                           .WithToken("Height", "32")
                           .Build();
        var input = new ThemeConfigBuilder()
                    .WithAlgorithms(ThemeAlgorithm.Default)
                    .WithToken("Alpha", "1.5")
                    .WithControl(ThemeConfigTestSchema.ButtonIdentity, buttonConfig)
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, schema);

        result.Success.ShouldBeTrue();
        var normalized = result.Config.ShouldNotBeNull();
        normalized.AlgorithmsSpecified.ShouldBeTrue();
        normalized.Algorithms.Select(static item => item.Algorithm).ShouldBe([ThemeAlgorithm.Default]);
        normalized.GlobalTokens.Single().Value.ShouldBe(1.5d);
        var button = normalized.Controls.Single();
        button.AlgorithmMode.ShouldBe(ControlAlgorithmMode.Global);
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
                    .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Compact)
                    .WithToken("Beta", "2.50")
                    .WithToken("Alpha", "1.50")
                    .Build();
        var second = new ThemeConfigBuilder()
                     .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Compact)
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
    public void Normalize_Treats_An_Empty_Global_Algorithm_List_As_Explicit_Default()
    {
        var input = new ThemeConfigBuilder()
                    .WithAlgorithms()
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, ThemeConfigTestSchema.Create());

        result.Success.ShouldBeTrue();
        result.Diagnostics.ShouldBeEmpty();
        var normalized = result.Config.ShouldNotBeNull();
        normalized.AlgorithmsSpecified.ShouldBeTrue();
        normalized.Algorithms.Select(static algorithm => algorithm.Algorithm).ShouldBe([ThemeAlgorithm.Default]);
    }

    [Fact]
    public void Normalize_Rejects_An_Unregistered_Algorithm()
    {
        var input = new ThemeConfigBuilder()
                    .WithAlgorithms(new ThemeAlgorithm("Test", "Unregistered"))
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, ThemeConfigTestSchema.Create());

        result.Success.ShouldBeFalse();
        result.Config.ShouldBeNull();
        result.Diagnostics.ShouldContain(static diagnostic =>
            diagnostic.Code == "ATMTHM4004" &&
            diagnostic.Path == "$.Algorithms[0]");
    }

    [Fact]
    public void Normalize_Rejects_Duplicate_Global_Algorithm_Values()
    {
        var input = new ThemeConfigBuilder()
                    .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Default)
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, ThemeConfigTestSchema.Create());

        result.Success.ShouldBeFalse();
        result.Config.ShouldBeNull();
        result.Diagnostics.ShouldContain(static diagnostic =>
            diagnostic.Code == "ATMTHM4004" &&
            diagnostic.Path == "$.Algorithms[1]");
    }

    [Fact]
    public void Normalize_Rejects_Duplicate_Control_Custom_Algorithm_Values()
    {
        var input = new ThemeConfigBuilder()
                    .WithControl(
                        ThemeConfigTestSchema.ButtonIdentity,
                        new ControlThemeConfigBuilder()
                            .WithAlgorithm(ControlAlgorithmMode.Custom)
                            .WithAlgorithms(ThemeAlgorithm.Compact, ThemeAlgorithm.Compact)
                            .Build())
                    .Build();

        var result = ThemeConfigNormalizer.Normalize(input, ThemeConfigTestSchema.Create());

        result.Success.ShouldBeFalse();
        result.Config.ShouldBeNull();
        result.Diagnostics.ShouldContain(static diagnostic =>
            diagnostic.Code == "ATMTHM4004" &&
            diagnostic.Path == "$.Controls['AtomUI:Button'].Algorithms[1]");
    }

    [Fact]
    public void Normalize_Returns_Diagnostics_Without_Publishing_Partial_Config()
    {
        var schema = ThemeConfigTestSchema.Create();
        var input = new ThemeConfigBuilder()
                    .WithAlgorithms(new ThemeAlgorithm("Test", "Unregistered"))
                    .WithToken("Alpha", "not-a-number")
                    .WithToken("Missing", "1")
                    .WithControl(
                        new ControlTokenIdentity("AtomUI", "Missing"),
                        new ControlThemeConfigBuilder().Build())
                    .WithControl(
                        ThemeConfigTestSchema.ButtonIdentity,
                        new ControlThemeConfigBuilder()
                            .WithAlgorithm(ControlAlgorithmMode.Custom)
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
    public void Normalize_Fingerprint_Changes_With_Algorithm_Revision()
    {
        var config = new ThemeConfigBuilder()
                     .WithAlgorithms(ThemeAlgorithm.Default)
                     .Build();

        var first = ThemeConfigNormalizer.Normalize(config, ThemeConfigTestSchema.Create()).Config.ShouldNotBeNull();
        var changed = ThemeConfigNormalizer.Normalize(
            config,
            ThemeConfigTestSchema.Create(defaultAlgorithmRevision: 2)).Config.ShouldNotBeNull();

        changed.Fingerprint.ShouldNotBe(first.Fingerprint);
    }

    [Fact]
    public void Config_Fingerprint_Is_Deterministic_For_Algorithm_Identities()
    {
        var config = new ThemeConfigBuilder()
                     .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Compact)
                     .Build();

        var first = ThemeConfigNormalizer.Normalize(config, ThemeConfigTestSchema.Create()).Config.ShouldNotBeNull();
        var second = ThemeConfigNormalizer.Normalize(config, ThemeConfigTestSchema.Create()).Config.ShouldNotBeNull();

        second.Fingerprint.ShouldBe(first.Fingerprint);
        // Pins cross-process stability: the fingerprint must never depend on string.GetHashCode().
        first.Fingerprint.Value.ShouldBe(0xE1D2F0E3E24383E2UL);
    }

    [Fact]
    public void Normalize_Rejects_A_Parser_Result_That_Does_Not_Match_The_Token_Value_Type()
    {
        var schema = ThemeConfigTestSchema.Create();
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
            schema.Algorithms);
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

internal static class ThemeConfigTestSchema
{
    internal static readonly ControlTokenIdentity ButtonIdentity = new("AtomUI", "Button");

    internal static ThemeSchemaRegistry Create(int defaultAlgorithmRevision = 1)
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
            static () => new TestControlToken(),
            static (_, _) => { });
        var algorithms = new[]
        {
            new ThemeAlgorithmDescriptor(
                ThemeAlgorithm.Compact,
                revision: 1,
                ThemeAppearanceEffect.Preserve,
                static () => new TestAlgorithm()),
            new ThemeAlgorithmDescriptor(
                ThemeAlgorithm.Default,
                revision: defaultAlgorithmRevision,
                ThemeAppearanceEffect.Light,
                static () => new TestAlgorithm())
        };
        return new ThemeSchemaRegistry(globalTokens, [control], algorithms);
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

    private sealed class TestControlToken : AbstractControlDesignToken
    {
    }

    private sealed class TestAlgorithm : IThemeAlgorithm
    {
        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
        {
        }
    }

    private enum TestTokenKind
    {
        Value
    }
}
