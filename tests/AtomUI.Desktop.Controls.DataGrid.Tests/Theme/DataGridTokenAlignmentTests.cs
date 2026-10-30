using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Theme;

// 锁定与 Ant Design 6.6.3 Table 的四处视觉对齐修正，防止回漂。
public class DataGridTokenAlignmentTests
{
    [Fact]
    public void Token_Values_Match_AntD_Table_Semantics()
    {
        var sharedToken = new DesignToken
        {
            PaddingXS             = new Thickness(8),
            PaddingSM             = new Thickness(12),
            ColorBorderSecondary  = Color.FromRgb(0xF0, 0xF0, 0xF0),
            ColorBgContainer      = Color.FromRgb(0xFF, 0xFF, 0xFF),
            ColorSplit            = Color.FromRgb(0x00, 0x00, 0x00)
        };
        var token = new DataGridToken();
        token.AssignEffectiveGlobalToken(sharedToken);
        token.CalculateTokenValues(isDarkMode: false);

        // AntD size.ts：middle 单元格 inline=paddingXS(8)、block=paddingSM(12)。
        token.CellPaddingMD.ShouldBe(
            new Thickness(
                sharedToken.PaddingXS.Left,
                sharedToken.PaddingSM.Top,
                sharedToken.PaddingXS.Right,
                sharedToken.PaddingSM.Bottom));

        // AntD index.ts: headerSplitColor = colorBorderSecondary。
        token.HeaderSplitColor.ShouldBe(sharedToken.ColorBorderSecondary);

        // AntD index.ts: filterDropdownBg = colorBgContainer。
        token.FilterDropdownBg.ShouldBe(sharedToken.ColorBgContainer);

        // AntD fixed.ts: inset 10px 0 8px -8px / inset -10px 0 8px -8px（阴影色 colorSplit）。
        token.LeftFrozenShadows.Count.ShouldBe(1);
        var left = token.LeftFrozenShadows[0];
        left.IsInset.ShouldBeTrue();
        left.OffsetX.ShouldBe(10);
        left.OffsetY.ShouldBe(0);
        left.Blur.ShouldBe(8);
        left.Spread.ShouldBe(-8);
        left.Color.ShouldBe(sharedToken.ColorSplit);

        token.RightFrozenShadows.Count.ShouldBe(1);
        var right = token.RightFrozenShadows[0];
        right.IsInset.ShouldBeTrue();
        right.OffsetX.ShouldBe(-10);
        right.OffsetY.ShouldBe(0);
        right.Blur.ShouldBe(8);
        right.Spread.ShouldBe(-8);
        right.Color.ShouldBe(sharedToken.ColorSplit);
    }
}
