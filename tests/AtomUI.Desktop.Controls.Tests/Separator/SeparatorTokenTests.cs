using AtomUI.Theme.DesignTokens;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Separator;

public class SeparatorTokenTests
{
    [Fact]
    public void Horizontal_Separator_Uses_Shared_Block_Margins_For_Each_Size()
    {
        var sharedToken = new DesignToken
        {
            UniformlyMarginXS = 8,
            UniformlyMargin   = 16,
            UniformlyMarginLG = 24
        };
        var separatorToken = new SeparatorToken();
        separatorToken.AssignEffectiveGlobalToken(sharedToken);

        separatorToken.RunDefaultCalculation(isDarkMode: false);

        separatorToken.HorizontalMarginBlockSM.ShouldBe(new Thickness(0, sharedToken.UniformlyMarginXS));
        separatorToken.HorizontalMarginBlock.ShouldBe(new Thickness(0, sharedToken.UniformlyMargin));
        separatorToken.HorizontalMarginBlockLG.ShouldBe(new Thickness(0, sharedToken.UniformlyMarginLG));
        separatorToken.HorizontalWithTextGutterMargin.ShouldBe(new Thickness(0, sharedToken.UniformlyMargin));
    }
}
