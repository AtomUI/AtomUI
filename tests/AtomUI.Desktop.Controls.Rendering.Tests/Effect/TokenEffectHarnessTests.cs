using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;
using AtomButton = AtomUI.Desktop.Controls.Button;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect;

public class TokenEffectHarnessTests
{
    [Fact]
    public void Visible_Global_Token_Override_Passes()
    {
        TokenEffectHarness.AssertHasEffect(
            () => new AtomButton { Content = "Go" },
            ButtonTokens.Identity,
            "ColorPrimary",
            control => ((AtomButton)control).ButtonType = ButtonType.Primary);
    }

    [Fact]
    public void Visible_Own_Token_Override_Passes()
    {
        TokenEffectHarness.AssertHasEffect(
            () => new AtomButton { Content = "Go" },
            ButtonTokens.Identity,
            "DefaultBg");
    }

    [Fact]
    public void Visible_Thin_Line_Token_Override_Passes()
    {
        TokenEffectHarness.AssertHasEffect(
            () => new AtomButton { Content = "Go" },
            ButtonTokens.Identity,
            "LineWidth");
    }

    [Fact]
    public void Token_Without_Visible_Effect_Fails()
    {
        // GutterToFlyout is an Unreferenced Button Own Token (ATOMUIGEN042 list): changing it must not repaint.
        var exception = Should.Throw<ShouldAssertException>(() => TokenEffectHarness.AssertHasEffect(
            () => new AtomButton { Content = "Go" },
            ButtonTokens.Identity,
            "GutterToFlyout"));
        exception.Message.ShouldContain("pixels changed inside the Control bounds");
    }

    [Theory]
    [InlineData("MotionDurationMid")]
    [InlineData("EnableWaveSpirit")]
    public void TimeSpan_And_Bool_Tokens_Assert_The_Read_Back_Value(string token)
    {
        TokenEffectHarness.AssertHasEffect(
            () => new AtomButton { Content = "Go" },
            ButtonTokens.Identity,
            token);
    }

    [Fact]
    public void Token_Of_A_Type_Without_Perturbation_Is_Not_Skipped()
    {
        var exception = Should.Throw<NotSupportedException>(() => TokenEffectHarness.AssertHasEffect(
            () => new AtomButton { Content = "Go" },
            ButtonTokens.Identity,
            "FontWeightStrong"));
        exception.Message.ShouldContain("FontWeightStrong");
    }

    [Fact]
    public void Unknown_Token_Name_Is_Rejected()
    {
        Should.Throw<ArgumentException>(() => TokenEffectHarness.AssertHasEffect(
            () => new AtomButton { Content = "Go" },
            ButtonTokens.Identity,
            "NoSuchToken"));
    }

    [Theory]
    [InlineData("LineWidth", 0.001)]
    [InlineData("LineWidthBold", 0.001)]
    [InlineData("ControlOutlineWidth", 0.001)]
    [InlineData("FocusVisualBorderThickness", 0.001)]
    [InlineData("HandleBorderThickness", 0.001)]
    [InlineData("ColorPrimary", 0.005)]
    [InlineData("ContentPadding", 0.005)]
    public void Thin_Line_Tokens_Use_The_Lower_Threshold(string token, double expected)
    {
        TokenEffectHarness.ResolveThreshold(token, TokenEffectHarness.DefaultThresholdRatio).ShouldBe(expected);
    }

    [Fact]
    public void Explicit_Lower_Threshold_Is_Kept_For_Thin_Line_Tokens()
    {
        TokenEffectHarness.ResolveThreshold("LineWidth", 0.0005).ShouldBe(0.0005);
    }
}

public class TokenPerturbationTests
{
    [Fact]
    public void Color_Is_Inverted_Keeping_Alpha()
    {
        TokenPerturbation.Perturb(Color.FromArgb(0xFF, 0x16, 0x77, 0xFF))
                         .ShouldBe(Color.FromArgb(0xFF, 0xE9, 0x88, 0x00));
    }

    [Fact]
    public void Near_Mid_Grey_Color_Becomes_A_Pure_Color()
    {
        var perturbed = (Color)TokenPerturbation.Perturb(Color.FromArgb(0xFF, 0x80, 0x7A, 0x88));
        perturbed.ShouldBe(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
    }

    [Fact]
    public void Mostly_Transparent_Color_Becomes_Opaque()
    {
        // rgba(0,0,0,0.04) inverted would stay invisible; the perturbation must be clearly visible.
        var perturbed = (Color)TokenPerturbation.Perturb(Color.FromArgb(0x0A, 0x00, 0x00, 0x00));
        perturbed.A.ShouldBe((byte)0xFF);
    }

    [Fact]
    public void Solid_Brush_Color_Is_Perturbed()
    {
        var perturbed = TokenPerturbation.Perturb(new SolidColorBrush(Color.FromArgb(0xFF, 0x16, 0x77, 0xFF)))
                                         .ShouldBeAssignableTo<ISolidColorBrush>();
        perturbed!.Color.ShouldBe(Color.FromArgb(0xFF, 0xE9, 0x88, 0x00));
        // The harness writes the perturbed value through the config string form; it must round-trip.
        ThemeTokenValueParser.Parse<IBrush>(ThemeTokenValueFormatter.Format<object>(perturbed))
                             .ShouldBeAssignableTo<ISolidColorBrush>()!.Color.ShouldBe(perturbed.Color);
    }

    [Theory]
    [InlineData(14.0, 28.0)]
    [InlineData(0.0, 8.0)]
    public void Double_Is_Doubled_Or_Set_To_Eight_From_Zero(double value, double expected)
    {
        TokenPerturbation.Perturb(value).ShouldBe(expected);
    }

    [Fact]
    public void Thickness_Grows_By_Four_Per_Side()
    {
        TokenPerturbation.Perturb(new Thickness(1, 2, 3, 4)).ShouldBe(new Thickness(5, 6, 7, 8));
    }

    [Fact]
    public void CornerRadius_Grows_By_Four_Per_Corner()
    {
        TokenPerturbation.Perturb(new CornerRadius(1, 2, 3, 4)).ShouldBe(new CornerRadius(5, 6, 7, 8));
    }

    [Fact]
    public void BoxShadows_Offset_And_Blur_Grow_By_Eight()
    {
        var shadows = new BoxShadows(new BoxShadow
        {
            OffsetX = 0,
            OffsetY = 2,
            Blur    = 4,
            Spread  = 1,
            Color   = Color.FromArgb(0x10, 0, 0, 0)
        });

        var perturbed = (BoxShadows)TokenPerturbation.Perturb(shadows);

        perturbed.Count.ShouldBe(1);
        perturbed[0].OffsetX.ShouldBe(8);
        perturbed[0].OffsetY.ShouldBe(10);
        perturbed[0].Blur.ShouldBe(12);
        perturbed[0].Spread.ShouldBe(1);
        perturbed[0].Color.ShouldBe(shadows[0].Color);
        ThemeTokenValueParser.Parse<BoxShadows>(ThemeTokenValueFormatter.Format<object>(perturbed)).ShouldBe(perturbed);
    }

    [Fact]
    public void TimeSpan_Is_Doubled()
    {
        TokenPerturbation.Perturb(TimeSpan.FromMilliseconds(200)).ShouldBe(TimeSpan.FromMilliseconds(400));
        TokenPerturbation.Perturb(TimeSpan.Zero).ShouldNotBe(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Bool_Is_Negated(bool value, bool expected)
    {
        TokenPerturbation.Perturb(value).ShouldBe(expected);
    }

    [Fact]
    public void Unsupported_Type_Throws()
    {
        Should.Throw<NotSupportedException>(() => TokenPerturbation.Perturb(FontWeight.Bold));
        Should.Throw<NotSupportedException>(() => TokenPerturbation.Perturb(3));
    }
}

public class EffectTestDataTests
{
    [Fact]
    public void Button_Rows_Contain_Active_Own_And_Consumed_Global_Tokens()
    {
        var rows = EffectTestData.For("AtomUI", "Button").ToList();

        rows.ShouldAllBe(row => row.Length == 2);
        rows.ShouldContain(row => (string)row[0] == "PrimaryColor" && (bool)row[1]);
        rows.ShouldContain(row => (string)row[0] == "ColorPrimary" && !(bool)row[1]);
        rows.Select(row => (string)row[0]).ShouldBeUnique();
    }

    [Fact]
    public void Button_Rows_Exclude_Unreferenced_Own_Tokens()
    {
        var tokens = EffectTestData.For("AtomUI", "Button").Select(row => (string)row[0]).ToList();

        tokens.ShouldNotContain("GutterToFlyout");
        tokens.ShouldNotContain("FontWeight");
    }

    [Fact]
    public void Selection_Excludes_Reserved_And_Unreferenced_And_Primitive_Entries()
    {
        var own = new[]
        {
            new EffectTestData.OwnTokenRow("AtomUI", "Demo", "Active", EffectTestData.OwnTokenStatus.Active),
            new EffectTestData.OwnTokenRow("AtomUI", "Demo", "Reserved", EffectTestData.OwnTokenStatus.Reserved),
            new EffectTestData.OwnTokenRow("AtomUI", "Demo", "Unused", EffectTestData.OwnTokenStatus.Unreferenced),
            new EffectTestData.OwnTokenRow("AtomUI", "Other", "Foreign", EffectTestData.OwnTokenStatus.Active)
        };
        var globals = new[]
        {
            new EffectTestData.GlobalConsumerRow("AtomUI", "Demo", "ColorText"),
            new EffectTestData.GlobalConsumerRow("AtomUI", "Demo", "ColorText"),
            new EffectTestData.GlobalConsumerRow("AtomUI", "Demo", "ColorBorder"),
            new EffectTestData.GlobalConsumerRow("AtomUI", EffectTestData.SharedPrimitiveId, "LineWidth"),
            new EffectTestData.GlobalConsumerRow("Other", "Demo", "FontSize")
        };

        var rows = EffectTestData.Select(own, globals, "AtomUI", "Demo");

        rows.ShouldBe([
            new EffectTestData.EffectToken("Active", true),
            new EffectTestData.EffectToken("ColorBorder", false),
            new EffectTestData.EffectToken("ColorText", false)
        ]);
    }

    [Fact]
    public void Unknown_Control_Is_Rejected()
    {
        Should.Throw<ArgumentException>(() => EffectTestData.For("AtomUI", "NoSuchControl").ToList());
    }
}
