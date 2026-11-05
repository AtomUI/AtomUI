using AtomUI.Theme.DesignTokens;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Animation.Easings;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuTokenTests
{
    [Fact]
    public void Item_Background_Motion_Easing_Defaults_To_Css_Ease()
    {
        var property = typeof(NavMenuToken).GetProperty("ItemBackgroundMotionEasing").ShouldNotBeNull();
        var navMenuToken = new NavMenuToken();
        var sharedToken = new DesignToken();
        navMenuToken.AssignEffectiveGlobalToken(sharedToken);

        NavMenuToken.EvaluateDefaults(navMenuToken,
            new ControlTokenDefaultContext(sharedToken, ThemeAppearance.Light), ReadOnlySpan<ulong>.Empty);

        var easing = property.GetValue(navMenuToken).ShouldBeOfType<SplineEasing>();
        easing.X1.ShouldBe(0.25);
        easing.Y1.ShouldBe(0.1);
        easing.X2.ShouldBe(0.25);
        easing.Y2.ShouldBe(1);
    }

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

        NavMenuToken.EvaluateDefaults(navMenuToken,
            new ControlTokenDefaultContext(sharedToken, ThemeAppearance.Light), ReadOnlySpan<ulong>.Empty);

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

        NavMenuToken.EvaluateDefaults(navMenuToken,
            new ControlTokenDefaultContext(sharedToken, ThemeAppearance.Light), ReadOnlySpan<ulong>.Empty);

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

        NavMenuToken.EvaluateDefaults(navMenuToken,
            new ControlTokenDefaultContext(sharedToken, ThemeAppearance.Light), ReadOnlySpan<ulong>.Empty);

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

        NavMenuToken.EvaluateDefaults(navMenuToken,
            new ControlTokenDefaultContext(sharedToken, ThemeAppearance.Light), ReadOnlySpan<ulong>.Empty);

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

        NavMenuToken.EvaluateDefaults(navMenuToken,
            new ControlTokenDefaultContext(sharedToken, ThemeAppearance.Light), ReadOnlySpan<ulong>.Empty);

        navMenuToken.ItemIconSize.ShouldBe(sharedToken.IconSize);
        navMenuToken.CollapsedIconSize.ShouldBe(sharedToken.IconSizeLG);
        navMenuToken.CollapsedIconSize.ShouldBeGreaterThan(navMenuToken.ItemIconSize);
    }
}
