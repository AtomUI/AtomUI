using AtomUI.Theme.DesignTokens;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class InputFoundationGlobalTokenTests
{
    [Fact]
    public void Alias_Calculation_Produces_Shared_Input_Foundation_Values()
    {
        var token = new DesignToken
        {
            FontSize                  = 14,
            FontSizeLG                = 16,
            RelativeLineHeight        = 1.5,
            RelativeLineHeightLG      = 1.5,
            ControlHeight             = 32,
            ControlHeightSM           = 24,
            ControlHeightLG           = 40,
            LineWidth                 = 1,
            SizeSM                    = 12,
            ControlPaddingHorizontal  = 12,
            ControlPaddingHorizontalSM = 8,
            ColorControlOutline       = Colors.Blue,
            ColorErrorOutline         = Colors.Red,
            ColorWarningOutline       = Colors.Orange
        };

        AtomUI.DesignLanguages.AntDesign.TokenEvaluation.AliasTokenRules.Apply(token);

        token.InputPadding.ShouldBe(new(11, 4.5));
        token.InputPaddingSM.ShouldBe(new(7, -0.5));
        token.InputPaddingLG.ShouldBe(new(11, 7));
        token.InputContentMargin.ShouldBe(new(1));
        token.InputActiveShadow.ShouldBe(new BoxShadows(new BoxShadow
        {
            Spread = 2,
            Color  = token.ColorControlOutline
        }));
        token.InputErrorActiveShadow.ShouldBe(new BoxShadows(new BoxShadow
        {
            Spread = 2,
            Color  = token.ColorErrorOutline
        }));
        token.InputWarningActiveShadow.ShouldBe(new BoxShadows(new BoxShadow
        {
            Spread = 2,
            Color  = token.ColorWarningOutline
        }));
    }
}
