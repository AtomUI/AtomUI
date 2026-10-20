using AtomUI.Theme.DesignTokens;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.SelectControl;

public class SelectTokenTests
{
    [Fact]
    public void MultiMode_Prefix_Indent_Complements_MultiModePadding_To_SingleMode_Horizontal_Padding()
    {
        var sharedToken = new DesignToken
        {
            LineWidth                  = 1,
            ControlHeight              = 32,
            ControlHeightSM            = 24,
            ControlHeightLG            = 40,
            FontHeight                 = 22,
            FontHeightLG               = 24,
            UniformlyPaddingSM         = 12,
            ControlPaddingHorizontalSM = 8,
            ControlPaddingHorizontal   = 12
        };
        var selectToken = new SelectToken();
        selectToken.AssignEffectiveGlobalToken(sharedToken);

        selectToken.CalculateTokenValues(isDarkMode: false);

        // 多选 frame 左内缩 + prefix 额外缩进 == 单选水平内缩（与 SharedToken InputPadding.Left 一致）
        (selectToken.MultiModePadding.Left + selectToken.MultiModePrefixIndent.Left)
            .ShouldBe(selectToken.SingleModePadding.Left);
        (selectToken.MultiModePaddingSM.Left + selectToken.MultiModePrefixIndentSM.Left)
            .ShouldBe(selectToken.SingleModePaddingSM.Left);
        (selectToken.MultiModePaddingLG.Left + selectToken.MultiModePrefixIndentLG.Left)
            .ShouldBe(selectToken.SingleModePaddingLG.Left);

        selectToken.MultiModePrefixIndent.Left.ShouldBe(7);   // 11 - 4
        selectToken.MultiModePrefixIndentSM.Left.ShouldBe(7); // 7 - 0
        selectToken.MultiModePrefixIndentLG.Left.ShouldBe(4); // 11 - 7
    }
}
