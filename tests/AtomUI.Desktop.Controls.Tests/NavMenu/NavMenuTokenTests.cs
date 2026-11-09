using AtomUI.Theme.DesignTokens;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuTokenTests
{
    [Fact]
    public void Item_Height_Uses_Large_Control_Height_To_Match_AntDesign_Menu()
    {
        var sharedToken = new DesignToken
        {
            ControlHeight   = 32,
            ControlHeightLG = 40
        };
        var navMenuToken = new NavMenuToken();
        navMenuToken.AssignEffectiveGlobalToken(sharedToken);

        navMenuToken.CalculateTokenValues(isDarkMode: false);

        navMenuToken.ItemHeight.ShouldBe(sharedToken.ControlHeightLG);
    }

    [Fact]
    public void Popup_Max_Height_Shows_At_Most_Eight_Menu_Items_By_Default()
    {
        var sharedToken = new DesignToken
        {
            ControlHeightLG = 40
        };
        var navMenuToken = new NavMenuToken();
        navMenuToken.AssignEffectiveGlobalToken(sharedToken);

        navMenuToken.CalculateTokenValues(isDarkMode: false);

        navMenuToken.MenuPopupMaxHeight.ShouldBe(navMenuToken.ItemHeight * 8);
    }

    [Fact]
    public void Vertical_Item_Gap_Uses_Collapsed_Block_Margin_To_Match_AntDesign_Menu()
    {
        var sharedToken = new DesignToken
        {
            UniformlyMarginXXS = 4
        };
        var navMenuToken = new NavMenuToken();
        navMenuToken.AssignEffectiveGlobalToken(sharedToken);

        navMenuToken.CalculateTokenValues(isDarkMode: false);

        navMenuToken.ItemContentMargin.ShouldBe(new Thickness(4, 0, 4, 4));
        navMenuToken.VerticalMenuContentPadding.ShouldBe(new Thickness(0, 4, 0, 0));
        navMenuToken.VerticalItemsPanelSpacing.ShouldBe(0);
        navMenuToken.VerticalChildItemsMargin.ShouldBe(new Thickness(0, 0, 0, 4));
    }

    [Fact]
    public void Inline_Collapsed_Width_Uses_Dedicated_Token_And_Keeps_Legacy_CollapsedWidth()
    {
        var sharedToken = new DesignToken
        {
            ControlHeight = 32
        };
        var navMenuToken = new NavMenuToken();
        navMenuToken.AssignEffectiveGlobalToken(sharedToken);

        navMenuToken.CalculateTokenValues(isDarkMode: false);

        navMenuToken.InlineCollapsedWidth.ShouldBe(48);
        navMenuToken.CollapsedWidth.ShouldBe(sharedToken.ControlHeight * 2);
    }

    [Fact]
    public void Inline_Collapsed_Icon_Uses_Large_Icon_Size_Level()
    {
        var sharedToken = new DesignToken
        {
            IconSize   = 14,
            IconSizeLG = 16
        };
        var navMenuToken = new NavMenuToken();
        navMenuToken.AssignEffectiveGlobalToken(sharedToken);

        navMenuToken.CalculateTokenValues(isDarkMode: false);

        navMenuToken.ItemIconSize.ShouldBe(sharedToken.IconSize);
        navMenuToken.CollapsedIconSize.ShouldBe(sharedToken.IconSizeLG);
        navMenuToken.CollapsedIconSize.ShouldBeGreaterThan(navMenuToken.ItemIconSize);
    }
}
