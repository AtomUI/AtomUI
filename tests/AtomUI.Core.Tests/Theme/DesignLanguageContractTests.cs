using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.ContractTests;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// The Ant Design language satisfies the design language contract under every declared variant, and the contract
/// reports each kind of violation of a deliberately broken language built on it.
/// </summary>
public class DesignLanguageContractTests
{
    [Fact]
    public void AntDesign_Satisfies_The_Contract_Under_Every_Variant()
    {
        var input = DesignLanguageContractInput.Capture(static builder => builder.UseAntDesign());

        DesignLanguageContract.Variants(new AntDesignLanguage().Axes).Count.ShouldBe(8);
        DesignLanguageContract.Verify(input, "AntDesign").ShouldBeEmpty();
    }

    [Fact]
    public void An_Unregistered_Language_Is_An_Identity_Violation()
    {
        var input = DesignLanguageContractInput.Capture(static builder => builder.UseAntDesign());

        var violation = DesignLanguageContract.Verify(input, "Missing").ShouldHaveSingleItem();
        violation.Check.ShouldBe(DesignLanguageContract.Identity);
    }

    [Fact]
    public void GlobalComplete_Reports_A_Global_Token_The_Language_Did_Not_Write()
    {
        var violations = Verify(BrokenLanguage.Mode.SkipsColorPrimaryHover);

        violations.ShouldContain(violation => violation.Check == DesignLanguageContract.GlobalComplete &&
                                              violation.TokenName == nameof(DesignToken.ColorPrimaryHover));
        violations.ShouldAllBe(violation => violation.Check == DesignLanguageContract.GlobalComplete);
    }

    [Fact]
    public void Deterministic_And_Pure_Report_A_Language_That_Reads_A_Static_Counter()
    {
        var violations = Verify(BrokenLanguage.Mode.ReadsStaticCounter);

        violations.ShouldContain(violation => violation.Check == DesignLanguageContract.Deterministic &&
                                              violation.TokenName == nameof(DesignToken.LineWidthBold));
        violations.ShouldContain(violation => violation.Check == DesignLanguageContract.Pure &&
                                              violation.TokenName == nameof(DesignToken.LineWidthBold));
    }

    [Fact]
    public void Readable_Reports_Text_With_The_Background_Color()
    {
        var violations = Verify(BrokenLanguage.Mode.Unreadable);

        violations.ShouldContain(violation => violation.Check == DesignLanguageContract.Readable &&
                                              violation.TokenName == nameof(DesignToken.ColorText));
        violations.Select(static violation => violation.Variant).Distinct().Count().ShouldBe(2);
    }

    [Fact]
    public void ContrastRatio_Matches_Known_Pairs()
    {
        ContrastRatio.Of(Colors.Black, Colors.White).ShouldBe(21, 1e-9);
        ContrastRatio.Of(Colors.White, Colors.Black).ShouldBe(21, 1e-9);
        ContrastRatio.Of(Color.Parse("#777777"), Colors.White).ShouldBe(4.48, 0.005);
        ContrastRatio.Of(Colors.White, Colors.White).ShouldBe(1, 1e-9);
        // A translucent foreground is composited over the background first: black at alpha 128 on white blends to
        // 255 × (1 − 128/255) = 127 per channel, i.e. #7F7F7F.
        ContrastRatio.Of(Color.FromArgb(128, 0, 0, 0), Colors.White)
                     .ShouldBe(ContrastRatio.Of(Color.FromRgb(127, 127, 127), Colors.White), 0.01);
    }

    private static IReadOnlyList<DesignLanguageContractViolation> Verify(BrokenLanguage.Mode mode)
    {
        var input = DesignLanguageContractInput.Capture(builder =>
            builder.Theme.AddDesignLanguage(new DesignLanguageDescriptor(new BrokenLanguage(mode), Array.Empty<TokenDescriptor>())));
        return DesignLanguageContract.Verify(input, "Broken");
    }

    /// <summary>
    /// Ant Design's derivation with one deliberate fault, under the light and dark default variants only.
    /// </summary>
    private sealed class BrokenLanguage(BrokenLanguage.Mode mode) : DesignLanguage<AntDesignToken>
    {
        internal enum Mode
        {
            SkipsColorPrimaryHover,
            ReadsStaticCounter,
            Unreadable
        }

        private static readonly AntDesignLanguage s_ant = new();
        private static int s_counter;

        public override string Id => "Broken";

        public override DesignLanguageAxes Axes { get; } = new([AntDesignLanguage.DefaultStyle], AntDesignLanguage.DefaultStyle,
            [DesignDensity.Default], [DesignIdiom.Desktop], []);

        public override void DefineSeeds(in DesignVariant variant, DesignToken seed, AntDesignToken languageSeed) =>
            s_ant.DefineSeeds(variant, seed, languageSeed);

        public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, AntDesignToken language) =>
            s_ant.DeriveLanguage(variant, seed, language);

        public override void DeriveGlobal(in TokenEvaluationContext<AntDesignToken> context, DesignToken token)
        {
            var hover = token.ColorPrimaryHover;
            s_ant.DeriveGlobal(context, token);
            switch (mode)
            {
                case Mode.SkipsColorPrimaryHover:
                    token.ColorPrimaryHover = hover;
                    break;
                case Mode.ReadsStaticCounter:
                    token.LineWidthBold = Interlocked.Increment(ref s_counter);
                    break;
                case Mode.Unreadable:
                    token.ColorText = token.ColorBgContainer;
                    break;
            }
        }
    }
}
