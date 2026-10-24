using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuThemeContractTests
{
    [Fact]
    public void Default_TreeDataTemplate_Binds_The_Entry_Composition_Contract()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuTheme.axaml");

        source.ShouldContain("<TreeDataTemplate ItemsSource=\"{Binding Entries}\"");
        source.ShouldNotContain("ItemsSource=\"{Binding Children}\"");
    }

    [Fact]
    public void NavMenu_Root_Background_Uses_Component_ItemBg_Token_Without_Right_Divider()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuTheme.axaml");

        source.ShouldContain("<Setter Property=\"Background\" Value=\"{atom:NavMenuTokenResource ItemBg}\" />");
        source.ShouldContain("<Setter Property=\"Background\" Value=\"{atom:NavMenuTokenResource DarkMenuBg}\" />");
        source.ShouldContain("<Setter Property=\"BorderThickness\" Value=\"0\" />");
        source.ShouldContain("<Setter Property=\"IsVisible\" Value=\"False\" />");
        source.ShouldNotContain("<Setter Property=\"BorderThickness\" Value=\"0,0,1,0\" />");
        source.ShouldNotContain("<Setter Property=\"Background\" Value=\"Transparent\" />");
        source.ShouldNotContain("<Setter Property=\"Background\" Value=\"{atom:SharedTokenResource ColorBgContainer}\" />");
        source.ShouldNotContain("<Setter Property=\"Background\" Value=\"{atom:NavMenuTokenResource DarkMenuPopupBg}\" />");
    }

    [Fact]
    public void Root_Template_Uses_Fixed_Collapsible_Header_And_Footer_Slots()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuTheme.axaml");

        source.ShouldContain("Name=\"PART_HeaderPresenter\"");
        source.ShouldContain("Content=\"{TemplateBinding Header}\"");
        source.ShouldContain("ContentTemplate=\"{TemplateBinding HeaderTemplate}\"");
        source.ShouldContain("IsVisible=\"{TemplateBinding Header, Converter={x:Static ObjectConverters.IsNotNull}}\"");
        source.ShouldContain("Name=\"PART_FooterPresenter\"");
        source.ShouldContain("Content=\"{TemplateBinding Footer}\"");
        source.ShouldContain("ContentTemplate=\"{TemplateBinding FooterTemplate}\"");
        source.ShouldContain("IsVisible=\"{TemplateBinding Footer, Converter={x:Static ObjectConverters.IsNotNull}}\"");
    }

    [Fact]
    public void Default_ItemsPanels_Consume_ItemSpacing_Without_Template_Descendant_Selectors()
    {
        var navMenuTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuTheme.axaml");
        var itemTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");
        var groupTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuGroupItemTheme.axaml");

        navMenuTheme.ShouldContain("Spacing=\"{TemplateBinding ItemSpacing}\"");
        navMenuTheme.ShouldContain("Property=\"EntryItemSpacing\"");
        navMenuTheme.ShouldContain("Value=\"{atom:NavMenuTokenResource VerticalItemsPanelSpacing}\"");
        navMenuTheme.ShouldNotContain("<Setter Property=\"ItemSpacing\" Value=\"0\" />");
        itemTheme.ShouldContain("Spacing=\"{TemplateBinding EntryItemSpacing}\"");
        groupTheme.ShouldContain("Spacing=\"{TemplateBinding EntryItemSpacing}\"");

        navMenuTheme.ShouldNotContain("ItemsPresenter#PART_ItemsPresenter StackPanel");
        itemTheme.ShouldNotContain("ItemsPresenter#ChildItemsPresenter StackPanel");
        groupTheme.ShouldNotContain("ItemsPresenter#PART_ItemsPresenter StackPanel");
    }

    [Fact]
    public void Inline_Submenu_Container_Uses_Submenu_Background_Tokens()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");

        source.ShouldContain("Name=\"PART_ChildItemsFrame\"");
        source.ShouldContain("Value=\"{atom:NavMenuTokenResource SubMenuItemBg}\"");
        source.ShouldContain("Value=\"{atom:NavMenuTokenResource DarkSubMenuItemBg}\"");
    }

    [Fact]
    public void Item_Background_Control_Is_Public_And_Forwarded_To_Internal_Menu_Parts()
    {
        var navMenuSource       = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs");
        var navMenuItemSource   = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenuItem.cs");
        var coordinatorSource   = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenuEntryContainerCoordinator.cs");
        var headerSource        = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Header/BaseNavMenuItemHeader.cs");
        var navMenuThemeSource  = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");

        navMenuSource.ShouldContain("public static readonly StyledProperty<bool> IsItemBackgroundEnabledProperty");
        navMenuSource.ShouldContain("AvaloniaProperty.Register<NavMenu, bool>(nameof(IsItemBackgroundEnabled), true)");
        navMenuSource.ShouldContain("public bool IsItemBackgroundEnabled");
        coordinatorSource.ShouldContain("BindUtils.RelayBind(menu, NavMenu.IsItemBackgroundEnabledProperty, menuItem, NavMenuItem.IsItemBackgroundEnabledProperty)");

        navMenuItemSource.ShouldContain("internal static readonly StyledProperty<bool> IsItemBackgroundEnabledProperty");
        coordinatorSource.ShouldContain("BindUtils.RelayBind(parentItem, NavMenuItem.IsItemBackgroundEnabledProperty, menuItem, NavMenuItem.IsItemBackgroundEnabledProperty)");
        coordinatorSource.ShouldContain("BindUtils.RelayBind(groupItem, NavMenuGroupItem.IsItemBackgroundEnabledProperty, menuItem, NavMenuItem.IsItemBackgroundEnabledProperty)");
        coordinatorSource.ShouldContain("menuItem.ClearValue(NavMenuItem.IsItemBackgroundEnabledProperty)");
        headerSource.ShouldContain("internal static readonly StyledProperty<bool> IsItemBackgroundEnabledProperty");
        navMenuThemeSource.ShouldContain("IsItemBackgroundEnabled=\"{TemplateBinding IsItemBackgroundEnabled}\"");
    }

    [Fact]
    public void Disabled_Item_Background_Mode_Gates_Item_Background_Without_Gating_Header_Backgrounds()
    {
        var headerThemeSource      = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/BaseNavMenuItemHeaderTheme.axaml");
        var horizontalThemeSource  = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/HorizontalNavMenuItemHeaderTheme.axaml");
        var navMenuItemThemeSource = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");

        headerThemeSource.ShouldNotContain("Selector=\"^[IsItemBackgroundEnabled=True]\"");
        headerThemeSource.ShouldNotContain("Selector=\"^[IsItemBackgroundEnabled=False]\"");
        horizontalThemeSource.ShouldNotContain("Selector=\"^[IsItemBackgroundEnabled=True]\"");
        horizontalThemeSource.ShouldNotContain("Selector=\"^[IsItemBackgroundEnabled=False]\"");

        navMenuItemThemeSource.ShouldContain("Selector=\"^[IsItemBackgroundEnabled=True]\"");
        navMenuItemThemeSource.ShouldContain("Selector=\"^[IsItemBackgroundEnabled=False]\"");
        navMenuItemThemeSource.ShouldContain("Selector=\"^ /template/ Border#PART_ChildItemsFrame\"");
        navMenuItemThemeSource.ShouldContain("<Setter Property=\"Background\" Value=\"Transparent\" />");
    }

    [Fact]
    public void Block_And_Popup_Item_Header_Backgrounds_Use_Content_Margin()
    {
        var inlineHeaderSource = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/InlineNavMenuItemHeaderTheme.axaml");
        var verticalHeaderSource = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/VerticalNavMenuItemHeaderTheme.axaml");
        var horizontalHeaderSource = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/HorizontalNavMenuItemHeaderTheme.axaml");

        inlineHeaderSource.ShouldContain("<Setter Property=\"Margin\" Value=\"{atom:NavMenuTokenResource ItemContentMargin}\" />");
        verticalHeaderSource.ShouldContain("<Setter Property=\"Margin\" Value=\"{atom:NavMenuTokenResource ItemContentMargin}\" />");
        horizontalHeaderSource.ShouldContain("Selector=\"^[IsTopLevel=False]\"");
        horizontalHeaderSource.ShouldContain("<Setter Property=\"Margin\" Value=\"{atom:NavMenuTokenResource ItemContentMargin}\" />");
    }

    [Fact]
    public void Popup_Frame_Uses_Component_Popup_Background_Token()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");

        source.ShouldContain("Value=\"{atom:NavMenuTokenResource MenuPopupBg}\"");
        source.ShouldNotContain("Value=\"{atom:SharedTokenResource ColorBgElevated}\"");
    }

    [Fact]
    public void Inline_And_Vertical_Pressed_State_Does_Not_Style_Header_From_Ancestor_NavMenuItem()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");

        source.ShouldNotContain(":pressed:not(:selected):not(^[IsDarkStyle=True])");
        source.ShouldNotContain("Value=\"{atom:NavMenuTokenResource ItemActiveBg}\"");
    }

    [Fact]
    public void Keyboard_Active_Visual_Is_Header_State_And_Does_Not_ReUse_Selected_State()
    {
        var headerSource      = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/BaseNavMenuItemHeaderTheme.axaml");
        var navMenuItemSource = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");

        navMenuItemSource.ShouldContain("IsKeyboardActive=\"{TemplateBinding IsKeyboardActive}\"");
        headerSource.ShouldContain("Selector=\"^[IsKeyboardActive=True]:not(^[IsSelected=True])\"");
        headerSource.ShouldNotContain("Selector=\"^[IsKeyboardActive=True]:not(^[IsSelected=True]):not(^[IsInSelectedPath=True])\"");
        headerSource.ShouldContain("Value=\"{atom:NavMenuTokenResource ItemActiveBg}\"");
        headerSource.ShouldContain("Value=\"{atom:NavMenuTokenResource DarkItemHoverBg}\"");
    }

    [Fact]
    public void Pointer_Hold_Visual_Uses_Only_Selected_Background_Without_ReUsing_Selected_State()
    {
        var headerSource      = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/BaseNavMenuItemHeaderTheme.axaml");
        var navMenuItemSource = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");

        navMenuItemSource.ShouldContain("IsPointerHold=\"{TemplateBinding IsPointerHold}\"");
        headerSource.ShouldContain("Selector=\"^[IsPointerHold=True]:not(^[IsSelected=True])\"");
        headerSource.ShouldContain("Value=\"{atom:NavMenuTokenResource ItemSelectedBg}\"");
        headerSource.ShouldContain("Value=\"{atom:NavMenuTokenResource DarkItemSelectedBg}\"");
        headerSource.ShouldNotContain(
            "Selector=\"^[IsPointerHold=True]:not(^[IsSelected=True])\">\n            <Setter Property=\"Foreground\"");
        headerSource.ShouldNotContain(
            "Selector=\"^[IsPointerHold=True]:not(^[IsSelected=True])\">\n                <Setter Property=\"Foreground\"");
        headerSource.ShouldNotContain("Selector=\"^[IsPointerHold=True]:not(^[IsSelected=True])\">\n            <Setter Property=\"Background\"\n                    Value=\"{atom:NavMenuTokenResource ItemHoverBg}\"");
    }

    [Fact]
    public void Submenu_Title_Hover_Uses_Foreground_And_Background_Tokens()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/BaseNavMenuItemHeaderTheme.axaml");

        source.ShouldContain("Value=\"{atom:NavMenuTokenResource ItemHoverColor}\"");
        source.ShouldContain("Value=\"{atom:NavMenuTokenResource ItemHoverBg}\"");
        source.ShouldContain("Value=\"{atom:NavMenuTokenResource DarkItemHoverColor}\"");
        source.ShouldContain("Value=\"{atom:NavMenuTokenResource DarkItemHoverBg}\"");
    }

    [Fact]
    public void NavMenu_Tokens_Keep_Root_And_Header_Background_Semantics_Separate()
    {
        var tokenSource = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenuToken.cs");
        var headerSource = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/BaseNavMenuItemHeaderTheme.axaml");

        tokenSource.ShouldContain("ItemBg                      = EffectiveGlobalToken.ColorBgContainer;");
        headerSource.ShouldContain("<Setter Property=\"Background\" Value=\"Transparent\" />");
    }

    [Fact]
    public void Inline_Collapsed_Uses_Effective_Mode_And_Axaml_Visual_State()
    {
        var navMenuSource       = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs");
        var coordinatorSource   = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenuEntryContainerCoordinator.cs");
        var navMenuThemeSource  = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuTheme.axaml");
        var navMenuItemTheme    = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");
        var verticalHeaderTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/VerticalNavMenuItemHeaderTheme.axaml");

        navMenuSource.ShouldContain("public static readonly StyledProperty<bool> IsInlineCollapsedProperty");
        navMenuSource.ShouldContain("public static readonly StyledProperty<double> InlineCollapsedWidthProperty");
        navMenuSource.ShouldContain("internal static readonly StyledProperty<double> InlineCollapsedLayoutWidthProperty");
        coordinatorSource.ShouldContain("BindUtils.RelayBind(menu, NavMenu.EffectiveModeProperty, menuItem, NavMenuItem.ModeProperty)");
        coordinatorSource.ShouldContain("BindUtils.RelayBind(menu, NavMenu.IsEffectiveInlineCollapsedProperty, menuItem, NavMenuItem.IsInlineCollapsedProperty)");
        coordinatorSource.ShouldContain("menuItem.ClearValue(NavMenuItem.ModeProperty)");
        coordinatorSource.ShouldContain("menuItem.ClearValue(NavMenuItem.IsInlineCollapsedProperty)");
        navMenuSource.ShouldNotContain("Layoutable.MaxWidthProperty");
        navMenuSource.ShouldNotContain("Layoutable.WidthProperty,\n                BindingMode.OneWay,\n                BindingPriority.Animation");
        navMenuSource.ShouldContain("WidthProperty.OverrideMetadata<NavMenu>");
        navMenuSource.ShouldContain("RunInlineCollapsedWidthMotion(");

        navMenuThemeSource.ShouldContain("<Setter Property=\"InlineCollapsedWidth\" Value=\"{atom:NavMenuTokenResource InlineCollapsedWidth}\" />");
        navMenuThemeSource.ShouldNotContain("<DoubleTransition Property=\"Width\"");
        navMenuThemeSource.ShouldNotContain("{Binding InlineCollapsedWidth");
        navMenuItemTheme.ShouldContain("IsInlineCollapsed=\"{TemplateBinding IsInlineCollapsed}\"");

        verticalHeaderTheme.ShouldContain("Selector=\"^[IsInlineCollapsed=True]\"");
        verticalHeaderTheme.ShouldContain("Value=\"{atom:NavMenuTokenResource CollapsedIconSize}\"");
        verticalHeaderTheme.ShouldContain("FirstCharacterConverter");
    }

    [Fact]
    public void Inline_Collapsed_Tooltip_Is_Projected_To_The_Visual_Header()
    {
        var navMenuSource       = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs");
        var nodeSource          = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenuNode.cs");
        var itemSource          = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenuItem.cs");
        var binderSource        = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenuItemContainerBinder.cs");
        var coordinatorSource   = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/NavMenuEntryContainerCoordinator.cs");
        var itemTheme           = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml");
        var verticalHeaderTheme = ReadRepoFile("src/AtomUI.Desktop.Controls/NavMenu/Themes/VerticalNavMenuItemHeaderTheme.axaml");

        nodeSource.ShouldContain("public static readonly DirectProperty<NavMenuNode, object?> TooltipProperty");
        nodeSource.ShouldContain("public static readonly DirectProperty<NavMenuNode, bool> IsTooltipEnabledProperty");
        navMenuSource.ShouldContain("public static readonly StyledProperty<bool> IsCollapsedTooltipEnabledProperty");
        itemSource.ShouldContain("internal static readonly DirectProperty<NavMenuItem, object?> EffectiveCollapsedTooltipProperty");
        binderSource.ShouldContain("NavMenuNode.TooltipProperty");
        binderSource.ShouldContain("NavMenuItem.NodeHeaderProperty");
        coordinatorSource.ShouldContain("NavMenu.IsCollapsedTooltipEnabledProperty");
        coordinatorSource.ShouldContain("menuItem.ClearValue(NavMenuItem.TooltipProperty)");
        itemTheme.ShouldContain("atom:ToolTip.Tip=\"{TemplateBinding EffectiveCollapsedTooltip}\"");
        itemTheme.ShouldContain("NodeHeader=\"{TemplateBinding NodeHeader}\"");
        verticalHeaderTheme.ShouldContain("Content=\"{TemplateBinding NodeHeader, Converter={x:Static atom:BaseNavMenuItemHeaderTheme.FirstCharacterConverter}}\"");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
