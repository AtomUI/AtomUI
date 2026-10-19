using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Menu;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Headless;
using Avalonia.Input;
using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;
using Avalonia.Media.TextFormatting;
using Avalonia.Controls.Documents;
using AtomUI.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUIMenu = AtomUI.Desktop.Controls.Menu;
using AtomUINavMenu = AtomUI.Desktop.Controls.NavMenu;
using NavMenuMode = AtomUI.Desktop.Controls.NavMenuMode;
using AtomUIWindow = Avalonia.Controls.Window;
using MenuShowCase = AtomUIGallery.ShowCases.Menu.MenuShowCase;

namespace AtomUIGallery.Tests.ShowCases;

public class MenuShowCasePageTests
{

    [Fact]
    public void Menu_ShowCase_Demonstrates_NavMenu_Structural_Entries_And_Root_Slots()
    {
        var pageSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Views/MenuShowCase.axaml");
        var zhCN = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Localization/zh-CN.xlf");
        var zhTW = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Localization/zh-TW.xlf");
        var enUS = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Localization/en-US.xlf");
        var viewModelSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/ViewModels/MenuViewModel.cs");
        var showCaseSource = ExtractShowCaseItem(pageSource, "NavMenuCompositionTitle");

        showCaseSource.ShouldContain("MenuShowCaseLangResource NavMenuCompositionDescription");
        showCaseSource.ShouldContain("ItemSpacing=\"4\"");
        showCaseSource.ShouldContain("<atom:NavMenu.Header>");
        showCaseSource.ShouldContain("<atom:NavMenu.Footer>");
        CountOccurrences(showCaseSource, "<atom:NavMenuGroup").ShouldBeGreaterThanOrEqualTo(3);
        CountOccurrences(showCaseSource, "<atom:NavMenuDivider").ShouldBeGreaterThanOrEqualTo(2);
        showCaseSource.ShouldContain("<atom:NavMenuNode.Entries>");
        showCaseSource.ShouldContain("IsInlineCollapsed=\"{Binding IsStructuredNavMenuCollapsed}\"");
        showCaseSource.ShouldContain("Click=\"HandleToggleStructuredNavMenuCollapsedClick\"");
        showCaseSource.ShouldNotContain("<atom:NavMenu.Styles>");

        viewModelSource.ShouldContain("public bool IsStructuredNavMenuCollapsed");
        viewModelSource.ShouldContain("HandleToggleStructuredNavMenuCollapsedClick");
        zhCN.ShouldContain("<target state=\"translated\">结构化导航菜单</target>");
        zhTW.ShouldContain("<target state=\"translated\">結構化導航菜單</target>");
        enUS.ShouldContain("<source>Structured nav menu</source>");
    }

    [Fact]
    public void Menu_ViewModel_Collapsed_Demo_State_Is_Independent()
    {
        var viewModel = new MenuViewModel(new TestScreen());

        viewModel.HandleToggleStructuredNavMenuCollapsedClick(null, null);

        viewModel.IsStructuredNavMenuCollapsed.ShouldBeTrue();
        viewModel.IsInlineCollapsed.ShouldBeFalse();

        viewModel.HandleToggleInlineCollapsedClick(null, null);

        viewModel.IsInlineCollapsed.ShouldBeTrue();
        viewModel.IsStructuredNavMenuCollapsed.ShouldBeTrue();
    }

    [Fact]
    public void Menu_ShowCase_NavMenuNode_Command_Uses_Explicit_Business_Keys_And_Displays_The_Last_Executed_Key()
    {
        var pageSource      = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Views/MenuShowCase.axaml");
        var viewModelSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/ViewModels/MenuViewModel.cs");
        var commandShowCaseSource = ExtractNavMenuNodeCommandShowCaseItem(pageSource);

        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource NavMenuNodeCommandTitle");
        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource NavMenuNodeCommandDescription");
        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource P2HeaderNavigationOne");
        commandShowCaseSource.ShouldContain("AntDesignIconProvider Kind=MailOutlined");
        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource P2HeaderNavigationTwo");
        commandShowCaseSource.ShouldContain("AntDesignIconProvider Kind=AppstoreOutlined");
        commandShowCaseSource.ShouldContain("IsEnabled=\"False\"");
        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource P2HeaderNavigationThreeSubmenu");
        commandShowCaseSource.ShouldContain("AntDesignIconProvider Kind=SettingOutlined");
        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource P2HeaderItemN1");
        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource P2HeaderItemN2");
        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource P2HeaderOptionN1");
        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource P2HeaderOptionN4");
        commandShowCaseSource.ShouldContain("MenuShowCaseLangResource P2HeaderNavigationFour");
        CountOccurrences(commandShowCaseSource, "Command=\"{Binding NavigateCommand}\"").ShouldBe(6);
        commandShowCaseSource.ShouldContain("ItemKey=\"command-navigation-one\"");
        commandShowCaseSource.ShouldContain("CommandParameter=\"navigation-one\"");
        commandShowCaseSource.ShouldContain("ItemKey=\"command-option-1\"");
        commandShowCaseSource.ShouldContain("CommandParameter=\"option-1\"");
        commandShowCaseSource.ShouldContain("ItemKey=\"command-option-4\"");
        commandShowCaseSource.ShouldContain("CommandParameter=\"option-4\"");
        commandShowCaseSource.ShouldContain("ItemKey=\"command-navigation-four\"");
        commandShowCaseSource.ShouldContain("CommandParameter=\"navigation-four\"");
        commandShowCaseSource.ShouldNotContain("CommandParameter=\"{Binding ItemKey}\"");
        commandShowCaseSource.ShouldContain("Text=\"{Binding LastCommandKey}\"");

        viewModelSource.ShouldContain("public ReactiveCommand<string, Unit> NavigateCommand { get; }");
        viewModelSource.ShouldContain("public string LastCommandKey");
        viewModelSource.ShouldContain("LastCommandKey = itemKey;");
    }

    [Fact]
    public void Menu_ViewModel_Command_Records_The_Explicit_Business_Key()
    {
        var viewModel = new MenuViewModel(new TestScreen());

        ((ICommand)viewModel.NavigateCommand).Execute("customer-overview");

        viewModel.LastCommandKey.ShouldBe("customer-overview");
    }

    [Fact]
    public void Menu_ShowCase_Scrollable_Menu_Can_Toggle_Popup_Scrolling()
    {
        var pageSource      = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Views/MenuShowCase.axaml");
        var viewModelSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/ViewModels/MenuViewModel.cs");
        var zhCN            = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Localization/zh-CN.xlf");
        var zhTW            = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Localization/zh-TW.xlf");
        var enUS            = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Localization/en-US.xlf");
        var scrollableShowCaseSource = ExtractShowCaseItem(pageSource, "ScrollableTitle");

        scrollableShowCaseSource.ShouldContain("MenuShowCaseLangResource ScrollableTitle");
        scrollableShowCaseSource.ShouldContain("MenuShowCaseLangResource P2TextEnablePopupScroll");
        scrollableShowCaseSource.ShouldContain("IsChecked=\"{Binding IsPopupScrollEnabled}\"");
        scrollableShowCaseSource.ShouldContain("IsScrollEnabled=\"{Binding IsPopupScrollEnabled}\"");
        scrollableShowCaseSource.ShouldNotContain("IsCheckedChanged=\"");
        CountOccurrences(scrollableShowCaseSource, "Header=\"{gallery:MenuShowCaseLangResource P2HeaderMenuItem}\"")
            .ShouldBe(12);

        viewModelSource.ShouldContain("private bool _isPopupScrollEnabled = true;");
        viewModelSource.ShouldContain("public bool IsPopupScrollEnabled");

        zhCN.ShouldContain("<target state=\"translated\">开启弹层滚动</target>");
        zhTW.ShouldContain("<target state=\"translated\">開啟彈層滾動</target>");
        enUS.ShouldContain("<source>Enable popup scrolling</source>");
    }

    [Fact]
    public void Menu_Semantic_Previews_Materialize_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new MenuShowCase
        {
            DataContext = new MenuViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1800, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            for (var i = 0; i < 3; i++)
            {
                Dispatcher.UIThread.RunJobs();
            }

            var previews = host.GetVisualDescendants().OfType<SemanticPartPreview>().ToArray();
            previews.Length.ShouldBe(3);
            var inlinePreview   = previews.Single(static candidate => candidate.Name == "NavMenuSemanticPreview");
            var verticalPreview = previews.Single(static candidate => candidate.Name == "VerticalNavMenuSemanticPreview");
            var menuPreview     = previews.Single(static candidate => candidate.Name == "MenuSemanticPreview");

            // 预览内容在跨程序集 AXAML 中构造，曾因引用 internal 钉住属性在挂载时直接抛
            // MethodAccessException；这里保证语义 tab 能真正实例化 owner 并列出部件。
            var inlineMenu = inlinePreview.SemanticOwner.ShouldBeOfType<AtomUINavMenu>();
            inlineMenu.Name.ShouldBe("NavMenuSemanticOwner");
            inlineMenu.Mode.ShouldBe(NavMenuMode.Inline);
            // 两个预览共用同一份 descriptor，都列出全部 12 个 Part；差异体现在目标实例数：
            // Inline 模板没有弹层节点（上游 inline 模式 popup 不生效），popup.root 解析为 0，
            // Vertical 预览由 code-behind 打开弹层后该 Part 才有可定位目标。
            SemanticPreviewPartCount(inlinePreview).ShouldBe(12);
            SemanticPreviewPartPaths(inlinePreview).ShouldContain("popup.root");

            var verticalMenu = verticalPreview.SemanticOwner.ShouldBeOfType<AtomUINavMenu>();
            verticalMenu.Name.ShouldBe("VerticalNavMenuSemanticOwner");
            verticalMenu.Mode.ShouldBe(NavMenuMode.Vertical);
            SemanticPreviewPartPaths(verticalPreview).ShouldContain("popup.root");
            SemanticPreviewPartCount(verticalPreview).ShouldBe(12);

            // 普通 Menu 预览：owner 是 Menu 自己（不再借用 DropdownButton 的契约），同样列出上游 12 个键。
            var previewMenu = menuPreview.SemanticOwner.ShouldBeOfType<AtomUIMenu>();
            previewMenu.Name.ShouldBe("MenuSemanticOwner");
            SemanticPreviewPartPaths(menuPreview).ShouldContain("popup.root");
            SemanticPreviewPartCount(menuPreview).ShouldBe(12);

            // 一级 item 容器是运行时生成物、不在 owner 模板内，只有 ">>" 路由能命中；
            // 同时确认 ItemClass 命中数与容器数一致，说明高亮落在真实容器上。
            HoverSemanticPartExclusive(previews, inlinePreview, "item");
            AssertAdornedClasses(page, "semantic-item");
            CollectAdornedElements(inlineMenu).Length.ShouldBe(Marked(inlineMenu, "semantic-item").Length);
            // 悬停一级 Part 不得命中子菜单容器的层级 marker。
            CollectAdornedElements(inlineMenu).ShouldNotContain(
                static element => element.ShouldBeAssignableTo<Control>().Classes.Contains("semantic-sub-menu-item"));

            HoverSemanticPartExclusive(previews, inlinePreview, "itemTitle");
            AssertAdornedClasses(page, "semantic-item-title");

            HoverSemanticPartExclusive(previews, inlinePreview, "list");
            AssertAdornedClasses(page, "semantic-list");

            // 夹具对齐上游语义示例的 openKeys：默认展开子菜单。Inline 模式在视觉树内展开，
            // 因此两级容器的 marker 与容器都在树内可解析，不需要跨视觉根注册弹层根。
            HoverSemanticPartExclusive(previews, inlinePreview, "subMenu.item");
            AssertAdornedClasses(page, "semantic-sub-menu-item");

            HoverSemanticPartExclusive(previews, inlinePreview, "subMenu.itemTitle");
            AssertAdornedClasses(page, "semantic-sub-menu-item-title");

            HoverSemanticPartExclusive(previews, inlinePreview, "subMenu.itemContent");
            AssertAdornedClasses(page, "semantic-sub-menu-item-content");

            HoverSemanticPartExclusive(previews, inlinePreview, "subMenu.itemIcon");
            AssertAdornedClasses(page, "semantic-sub-menu-item-icon");

            // Inline 模式没有弹层节点，popup.root 目标数为 0（上游同样是 inline 不生效）。
            HoverSemanticPartExclusive(previews, inlinePreview, "popup.root");
            AssertAdornedClasses(page, []);

            // Vertical 预览：code-behind 在指针移入预览后通过公开的 INavMenuItem 契约打开子菜单，
            // 并把弹层的 Popup.Child 注册进 AdditionalRoots（同 DropdownButton / InfoFlyout 的既定
            // 模式）。弹层只在指针移入后打开，因为 AtomUI Popup 会在放置目标跑出 TopLevel 可视矩形时
            // 主动关闭弹层，而本预览在页面初始状态位于折叠线以外。这里模拟真实指针移入预览。
            SemanticPreviewPartPaths(verticalPreview).ShouldContain("popup.root");
            SemanticPreviewPartCount(verticalPreview).ShouldBe(12);

            // 钉住打开由 AXAML 声明、由 NavMenu 自身在公开属性驱动下完成。
            verticalMenu.IsPopupPinnedOpen.ShouldBeTrue();
            for (var i = 0; i < 25; i++)
            {
                Dispatcher.UIThread.RunJobs();
                TopLevel.GetTopLevel(page)!.UpdateLayout();
            }

            // 弹层打开状态直接取容器模板里的 Popup：overlay 模式下弹层内容挂在 overlay 层，
            // 不能用 owner 子树枚举判断。
            var pinnedContainer = verticalMenu.ContainerFromItem(verticalMenu.Items.OfType<object>().ElementAt(1))
                                              .ShouldNotBeNull();
            var candidates = pinnedContainer.GetVisualDescendants()
                                            .OfType<Avalonia.Controls.Primitives.Popup>().ToArray();
            var states = string.Join(",", candidates.Select(static x => $"{(x.Name ?? "?")}:{x.IsOpen}"));
            candidates.Length.ShouldBe(1, $"popups in submenu container: [{states}]");
            var pinnedPopup = candidates[0];
            pinnedPopup.IsOpen.ShouldBeTrue(
                "the pinned vertical preview must keep its submenu popup open");

            // 钉住语义：普通关闭请求（业务 open state 写回 false、Close）不得收起弹层。
            var pinnedNavItem = pinnedContainer.ShouldBeAssignableTo<AtomUI.Desktop.Controls.INavMenuItem>();
            pinnedNavItem.IsSubMenuOpen = false;
            pinnedNavItem.Close();
            for (var i = 0; i < 5; i++)
            {
                Dispatcher.UIThread.RunJobs();
                TopLevel.GetTopLevel(page)!.UpdateLayout();
            }

            pinnedPopup.IsOpen.ShouldBeTrue(
                "the pinned vertical preview must keep its submenu popup open");

            HoverSemanticPartExclusive(previews, verticalPreview, "popup.root");
            AssertAdornedClasses(page, "semantic-popup-root", requireNonEmpty: true);

            HoverSemanticPartExclusive(previews, verticalPreview, "subMenu.item");
            AssertAdornedClasses(page, "semantic-sub-menu-item", requireNonEmpty: true);

            HoverSemanticPartExclusive(previews, verticalPreview, "subMenu.itemContent");
            AssertAdornedClasses(page, "semantic-sub-menu-item-content", requireNonEmpty: true);

            HoverSemanticPartExclusive(previews, verticalPreview, "item");
            AssertAdornedClasses(page, "semantic-item", requireNonEmpty: true);

            HoverSemanticPartExclusive(previews, verticalPreview, "itemTitle");
            AssertAdornedClasses(page, "semantic-item-title", requireNonEmpty: true);

            HoverSemanticPartExclusive(previews, verticalPreview, "list");
            AssertAdornedClasses(page, "semantic-list", requireNonEmpty: true);

            HoverSemanticPartExclusive(previews, verticalPreview, "itemContent");
            AssertAdornedClasses(page, "semantic-item-content", requireNonEmpty: true);

            // 普通 Menu 预览：顶层项是菜单栏项（TopLevelMenuItemTheme），钉住打开的文件子菜单提供弹层部件。
            previewMenu.IsPopupPinnedOpen.ShouldBeTrue();
            for (var i = 0; i < 25; i++)
            {
                Dispatcher.UIThread.RunJobs();
                TopLevel.GetTopLevel(page)!.UpdateLayout();
            }

            // 一级 item / itemIcon / itemContent：菜单栏项只带一级 marker，不能命中子菜单 marker。
            HoverSemanticPartExclusive(previews, menuPreview, "item");
            AssertAdornedClasses(page, "semantic-item", requireNonEmpty: true);
            CollectAdornedElements(previewMenu)
                .ShouldNotContain(static element => element.ShouldBeAssignableTo<Control>()
                                                           .Classes.Contains("semantic-sub-menu-item"));

            // 菜单栏是 Horizontal 语义：顶层 itemTitle / list 在上游同样不生效，解析为 0。
            HoverSemanticPartExclusive(previews, menuPreview, "itemTitle");
            AssertAdornedClasses(page, []);
            HoverSemanticPartExclusive(previews, menuPreview, "list");
            AssertAdornedClasses(page, []);

            // 弹层侧部件：在打开的文件子菜单里真实存在。
            HoverSemanticPartExclusive(previews, menuPreview, "popup.root");
            AssertAdornedClasses(page, "semantic-popup-root", requireNonEmpty: true);
            HoverSemanticPartExclusive(previews, menuPreview, "subMenu.item");
            AssertAdornedClasses(page, "semantic-sub-menu-item", requireNonEmpty: true);
            HoverSemanticPartExclusive(previews, menuPreview, "subMenu.itemIcon");
            AssertAdornedClasses(page, "semantic-sub-menu-item-icon", requireNonEmpty: true);
            HoverSemanticPartExclusive(previews, menuPreview, "subMenu.itemContent");
            AssertAdornedClasses(page, "semantic-sub-menu-item-content", requireNonEmpty: true);
            HoverSemanticPartExclusive(previews, menuPreview, "subMenu.itemTitle");
            AssertAdornedClasses(page, "semantic-sub-menu-item-title", requireNonEmpty: true);
            HoverSemanticPartExclusive(previews, menuPreview, "subMenu.list");
            AssertAdornedClasses(page, "semantic-sub-menu-list", requireNonEmpty: true);

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(
                page.GetVisualDescendants().OfType<SemanticPartPreview>(),
                static candidate => Assert.False(candidate.IsEffectivelyVisible));
        });
    }

    [Fact]
    public void Menu_Semantic_Style_Example_Uses_The_Menu_Generated_Part_Styles()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Views/MenuShowCase.axaml");
        var example = ExtractSemanticPartExampleItem(source);

        // 普通 Menu 的样式块并入同一示例项；owner-scoped 声明，不得依赖 PART_*。
        example.ShouldContain("Selector=\"atom|Menu.semantic-style-demo\"");
        example.ShouldContain("Selector=\"atom|Menu.semantic-sub-menu-style-demo\"");
        example.ShouldContain("<atom:MenuItemStyle x:SetterTargetType=\"MenuItem\">");
        example.ShouldContain("<atom:MenuItemIconStyle x:SetterTargetType=\"atom:IconPresenter\">");
        example.ShouldContain("<atom:MenuItemContentStyle x:SetterTargetType=\"ContentPresenter\">");
        example.ShouldContain("<atom:MenuPopupRootStyle x:SetterTargetType=\"Border\">");
        example.ShouldContain("<atom:MenuSubMenuItemStyle x:SetterTargetType=\"MenuItem\">");
        example.ShouldContain("<atom:MenuSubMenuItemIconStyle x:SetterTargetType=\"atom:IconPresenter\">");
        example.ShouldContain("<atom:MenuSubMenuItemContentStyle x:SetterTargetType=\"ContentPresenter\">");
        example.ShouldContain("<atom:MenuSubMenuItemTitleStyle x:SetterTargetType=\"ContentPresenter\">");
        example.ShouldContain("<atom:MenuSubMenuListStyle x:SetterTargetType=\"ItemsPresenter\">");
        // 弹层完全收起、靠点击展开，示例不得钉住弹层。
        example.ShouldNotContain("IsPopupPinnedOpen");
        example.ShouldNotContain("PART_");
    }

    [Fact]
    public void Menu_Semantic_Part_Example_Uses_Published_Parts_Through_Generated_Styles()
    {
        var source         = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Views/MenuShowCase.axaml");
        var english        = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Navigation/Menu/Localization/en-US.xlf");
        var semanticSource = ExtractSemanticPartExampleItem(source);

        semanticSource.ShouldContain("SourceKey=\"menu-semantic-part\"");
        semanticSource.ShouldContain("MenuShowCaseLangResource SemanticPartStyleTitle");
        semanticSource.ShouldContain("MenuShowCaseLangResource SemanticPartStyleDescription");
        semanticSource.ShouldContain("Selector=\"atom|NavMenu.semantic-style-demo\"");
        semanticSource.ShouldContain("Selector=\"atom|NavMenu.semantic-inline-style-demo\"");
        semanticSource.ShouldContain("<atom:NavMenuItemStyle x:SetterTargetType=\"HeaderedSelectingItemsControl\">");
        semanticSource.ShouldContain("<atom:NavMenuItemIconStyle x:SetterTargetType=\"atom:IconPresenter\">");
        semanticSource.ShouldContain("<atom:NavMenuItemContentStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<atom:NavMenuItemTitleStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<atom:NavMenuListStyle x:SetterTargetType=\"ItemsPresenter\">");
        semanticSource.ShouldContain("<atom:NavMenuSubMenuItemStyle x:SetterTargetType=\"HeaderedSelectingItemsControl\">");
        semanticSource.ShouldContain("<atom:NavMenuSubMenuItemTitleStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<atom:NavMenuSubMenuItemContentStyle x:SetterTargetType=\"ContentPresenter\">");
        semanticSource.ShouldContain("<atom:NavMenuSubMenuListStyle x:SetterTargetType=\"ItemsPresenter\">");
        semanticSource.ShouldContain("<atom:NavMenuPopupRootStyle x:SetterTargetType=\"Border\">");
        semanticSource.ShouldContain("Classes=\"semantic-style-demo\"");
        semanticSource.ShouldContain("Classes=\"semantic-inline-style-demo\"");
        // 外层垂直容器内两行水平 StackPanel：第一行两个 NavMenu，第二行两个 Menu。
        semanticSource.ShouldContain("<StackPanel Spacing=\"16\">");
        CountOccurrences(semanticSource, "<StackPanel Orientation=\"Horizontal\" Spacing=\"16\">")
                     .ShouldBe(2);
        CountOccurrences(semanticSource, "VerticalAlignment=\"Top\"").ShouldBe(4);
        // 普通 Menu 两个菜单栏同样属于本示例项。
        semanticSource.ShouldContain("Classes=\"semantic-style-demo\"");
        semanticSource.ShouldContain("Classes=\"semantic-sub-menu-style-demo\"");
        // 示例不得依赖 PART_* 或具体模板节点。
        semanticSource.ShouldNotContain("PART_");

        foreach (var key in new[]
                 {
                     "SemanticPartStyleTitle",
                     "SemanticPartStyleDescription",
                     "P2HeaderNavigationThree"
                 })
        {
            english.ShouldContain($"<unit id=\"{key}\">");
        }
    }

    private static string ExtractSemanticPartExampleItem(string source)
    {
        const string sourceKeyMarker = "menu-semantic-part";
        const string itemCloseMarker = "</gallery:ShowCaseItem>";

        var keyIndex = source.IndexOf(sourceKeyMarker, StringComparison.Ordinal);
        keyIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", keyIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        // 只取该示例自身的 ShowCaseItem，不把其后并列的示例项一并纳入。
        var itemCloseStart = source.IndexOf(itemCloseMarker, keyIndex, StringComparison.Ordinal);
        itemCloseStart.ShouldBeGreaterThan(keyIndex);

        return source[itemStart..itemCloseStart];
    }

    [Fact]
    public void Menu_Semantic_Style_Example_Applies_The_Official_Style_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new MenuShowCase
        {
            DataContext = new MenuViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1400, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item  = panel.Children
                             .OfType<ShowCaseItem>()
                             .Single(static candidate => candidate.SourceKey == "menu-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var verticalMenu = page.GetVisualDescendants()
                                   .OfType<AtomUINavMenu>()
                                   .Single(static menu => menu.Classes.Contains("semantic-style-demo"));
            AssertSolidColor(verticalMenu.Background, "#FAFAFA");
            AssertSolidColor(verticalMenu.BorderBrush, "#D9D9D9");
            verticalMenu.BorderThickness.ShouldBe(new Thickness(1));
            verticalMenu.CornerRadius.ShouldBe(new CornerRadius(8));
            verticalMenu.Padding.ShouldBe(new Thickness(8));

            var itemContainer = Marked(verticalMenu, "semantic-item").First()
                                                                     .ShouldBeAssignableTo<TemplatedControl>();
            AssertSolidColor(itemContainer.Foreground, "#1677FF");
            itemContainer.MinHeight.ShouldBe(36);

            var itemIcon = Marked(verticalMenu, "semantic-item-icon").First()
                                                                 .ShouldBeAssignableTo<IconPresenter>();
            AssertSolidColor(itemIcon.IconBrush, "#1677FF");

            var itemContent = Marked(verticalMenu, "semantic-item-content").First()
                                                                          .ShouldBeAssignableTo<ContentPresenter>();
            itemContent.FontWeight.ShouldBe(FontWeight.Bold);

            TextElement.GetFontWeight(Marked(verticalMenu, "semantic-item-title").First())
                       .ShouldBe(FontWeight.SemiBold);

            // Inline 示例在视觉树内展开，子菜单条目的生成样式同样可断言。
            var inlineMenu = page.GetVisualDescendants()
                                 .OfType<AtomUINavMenu>()
                                 .Single(static menu => menu.Classes.Contains("semantic-inline-style-demo"));
            AssertSolidColor(inlineMenu.Background, "#F0F5FF");
            AssertSolidColor(
                Marked(inlineMenu, "semantic-sub-menu-item").First()
                                                            .ShouldBeAssignableTo<TemplatedControl>()
                                                            .Foreground,
                "#FA541C");
            // 一级项的 header 同时带两级 marker，因此 subMenu.* 样式只应命中真正位于子菜单
            // 容器内的节点；这里按容器祖先过滤，避免误取一级项上的同名节点。
            MarkedWithin(inlineMenu, "semantic-sub-menu-item-content", "semantic-sub-menu-item")
                .First()
                .ShouldBeAssignableTo<ContentPresenter>()
                .FontWeight
                .ShouldBe(FontWeight.Bold);
            TextElement.GetFontWeight(
                    MarkedWithin(inlineMenu, "semantic-sub-menu-item-title", "semantic-sub-menu-group").First())
                       .ShouldBe(FontWeight.SemiBold);

            // 同一示例项里的普通 Menu：菜单栏一级项样式直接可断言。
            var plainMenu = page.GetVisualDescendants()
                                .OfType<AtomUIMenu>()
                                .Single(static menu => menu.Classes.Contains("semantic-style-demo"));
            AssertSolidColor(plainMenu.Background, "#FAFAFA");
            AssertSolidColor(plainMenu.BorderBrush, "#D9D9D9");
            plainMenu.BorderThickness.ShouldBe(new Thickness(1));
            plainMenu.CornerRadius.ShouldBe(new CornerRadius(8));

            var plainItem = Marked(plainMenu, "semantic-item").First()
                                                             .ShouldBeAssignableTo<TemplatedControl>();
            AssertSolidColor(plainItem.Foreground, "#1677FF");
            var plainItemIcon = Marked(plainMenu, "semantic-item-icon").First()
                                                                       .ShouldBeAssignableTo<IconPresenter>();
            AssertSolidColor(plainItemIcon.IconBrush, "#1677FF");

            // 菜单栏一级项的图标必须真正渲染：顶层模板若只给 IconPresenter 设 Margin 而不设尺寸，
            // 图标量到 0x0，肉眼不可见却仍占位，菜单项文字被无谓右移。
            plainItemIcon.Width.ShouldBeGreaterThan(0);
            plainItemIcon.Height.ShouldBe(plainItemIcon.Width);
            plainItemIcon.Bounds.Width.ShouldBeGreaterThan(0);
            plainItemIcon.Bounds.Height.ShouldBeGreaterThan(0);

            Marked(plainMenu, "semantic-item-content").First()
                                                      .ShouldBeAssignableTo<ContentPresenter>()
                                                      .FontWeight
                                                      .ShouldBe(FontWeight.Bold);

            // 弹层默认完全收起：靠点击展开，不使用钉住。
            plainMenu.IsPopupPinnedOpen.ShouldBeFalse();
            IsPopupOpen(plainMenu).ShouldBeFalse();

            // 第二个菜单承载 subMenu.* 部件样式演示；它默认收起。
            var subMenuStyleMenu = page.GetVisualDescendants()
                                       .OfType<AtomUIMenu>()
                                       .Single(static menu => menu.Classes.Contains("semantic-sub-menu-style-demo"));
            AssertSolidColor(subMenuStyleMenu.Background, "#F0F5FF");
            IsPopupOpen(subMenuStyleMenu).ShouldBeFalse();

            void PumpLayout(int iterations)
            {
                for (var i = 0; i < iterations; i++)
                {
                    Dispatcher.UIThread.RunJobs();
                    TopLevel.GetTopLevel(page)!.UpdateLayout();
                }
            }

            // overlay 模式下弹层内容挂在 overlay 层，不在 owner 子树里；先展开再取弹层 Child 断言。
            // 弹层默认完全收起、靠点击展开，这里用与点击等价的 IsSubMenuOpen 驱动。
            Control OpenSubMenuPopup(Avalonia.Controls.MenuItem owner)
            {
                owner.BringIntoView();
                PumpLayout(5);
                owner.SetCurrentValue(Avalonia.Controls.MenuItem.IsSubMenuOpenProperty, true);
                PumpLayout(12);
                var popup = owner.GetVisualDescendants()
                                 .OfType<Avalonia.Controls.Primitives.Popup>()
                                 .Single(static candidate => candidate.IsOpen);
                return popup.Child.ShouldNotBeNull();
            }

            // 该示例项位于长页面底部，必须先滚入可视区：AtomUI Popup 会在放置目标跑出 TopLevel
            // 可视矩形时按有效性规则拒绝打开，弹层内容也就不会实例化。
            var scrollViewer = page.GetVisualDescendants().OfType<AvaloniaScrollViewer>().First();
            scrollViewer.ScrollToEnd();
            PumpLayout(5);
            scrollViewer.Offset.Y.ShouldBeGreaterThan(0);

            // popup.root 由 semantic-style-demo 声明：该菜单的弹层根 Border 就是弹层 Child 本身。
            var popupRootOwner = Marked(plainMenu, "semantic-item")
                .OfType<AtomUI.Desktop.Controls.MenuItem>()
                .First(static candidate => candidate.HasSubMenu);
            var popupRootFrame = OpenSubMenuPopup(popupRootOwner).ShouldBeAssignableTo<Border>();
            popupRootFrame.Classes.Contains("semantic-popup-root").ShouldBeTrue();
            AssertSolidColor(popupRootFrame.BorderBrush, "#D9D9D9");
            popupRootFrame.BorderThickness.ShouldBe(new Thickness(1));
            popupRootFrame.CornerRadius.ShouldBe(new CornerRadius(8));
            popupRootFrame.Padding.ShouldBe(new Thickness(8));

            // subMenu.* 由 semantic-sub-menu-style-demo 声明：在它自己的弹层内断言。
            var subMenuItemOwner = Marked(subMenuStyleMenu, "semantic-item")
                .OfType<AtomUI.Desktop.Controls.MenuItem>()
                .First(static candidate => candidate.HasSubMenu);
            var popupContent = OpenSubMenuPopup(subMenuItemOwner);
            AssertSolidColor(
                Marked(popupContent, "semantic-sub-menu-item").First()
                                                              .ShouldBeAssignableTo<TemplatedControl>()
                                                              .Foreground,
                "#FA541C");
            AssertSolidColor(
                Marked(popupContent, "semantic-sub-menu-item-icon").First()
                                                                    .ShouldBeAssignableTo<IconPresenter>()
                                                                    .IconBrush,
                "#FA541C");
            Marked(popupContent, "semantic-sub-menu-item-content")
                .First()
                .ShouldBeAssignableTo<ContentPresenter>()
                .FontWeight
                .ShouldBe(FontWeight.Bold);

            // 弹层内所有菜单行的文字左边缘必须一致：MenuItem 的图标列参与 SharedSizeGroup，
            // 但分组容器（MenuItemGroup）自带的 ItemsPresenter 位于 popup 的共享尺寸作用域之外，
            // 它的子项图标列不会被“同层有图标的兄弟”撑开。若示例子项在有/无图标之间混用，
            // 分组行的图标列量到 0、文字左移，与直接子项的文字对不齐。
            var popupFrame = popupContent.ShouldBeAssignableTo<Border>();
            var textLeftEdges = popupContent.GetVisualDescendants()
                                            .OfType<AtomUI.Desktop.Controls.MenuItem>()
                                            .Select(item => item.GetVisualDescendants()
                                                                .OfType<ContentPresenter>()
                                                                .FirstOrDefault(presenter =>
                                                                    presenter.Name == "ItemTextPresenter"))
                                            .Where(presenter => presenter is not null)
                                            .Select(presenter => presenter!
                                                 .TranslatePoint(new Point(0, 0), popupFrame)
                                                 .ShouldNotBeNull()
                                                 .X)
                                            .Distinct()
                                            .ToArray();
            textLeftEdges.Length.ShouldBe(1,
                "every submenu row must share the same text left edge; mixed icon usage must not shift group rows");
        });
    }

    private static Control[] Marked(Visual root, string semanticClass)
    {
        return root.GetVisualDescendants()
                   .OfType<Control>()
                   .Where(control => control.Classes.Contains(semanticClass))
                   .ToArray();
    }

    private static Control[] MarkedWithin(Visual root, string semanticClass, string containerClass)
    {
        return root.GetVisualDescendants()
                   .OfType<Control>()
                   .Where(control => control.Classes.Contains(semanticClass) &&
                                     control.GetVisualAncestors()
                                            .Any(ancestor => ancestor.Classes.Contains(containerClass)))
                   .ToArray();
    }

    private static bool IsPopupOpen(Control owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<Avalonia.Controls.Primitives.Popup>()
                    .Any(static popup => popup.IsOpen);
    }

    private static void AssertSolidColor(IBrush? brush, string expected)
    {
        brush.ShouldNotBeNull()
             .ShouldBeAssignableTo<ISolidColorBrush>()
             .ShouldNotBeNull()
             .Color.ShouldBe(Color.Parse(expected));
    }

    private static void HoverSemanticPartExclusive(
        IReadOnlyList<SemanticPartPreview> previews,
        SemanticPartPreview target,
        string path)
    {
        foreach (var preview in previews)
        {
            foreach (var item in SemanticPreviewItems(preview).ToArray())
            {
                typeof(SemanticPartPreview)
                    .GetMethod("SetHoveredPart", BindingFlags.Instance | BindingFlags.NonPublic)
                    .ShouldNotBeNull()
                    .Invoke(preview, [item, false]);
            }
        }

        Dispatcher.UIThread.RunJobs();
        HoverSemanticPart(target, path);
    }

    private static void HoverSemanticPart(SemanticPartPreview preview, string path)
    {
        var part = SemanticPreviewPart(preview, path);
        typeof(SemanticPartPreview)
            .GetMethod("SetHoveredPart", BindingFlags.Instance | BindingFlags.NonPublic)
            .ShouldNotBeNull()
            .Invoke(preview, [part, true]);
        Dispatcher.UIThread.RunJobs();
    }

    private static void AssertAdornedClasses(Visual withinTree, string expectedClass, bool requireNonEmpty = false)
    {
        AssertAdornedClasses(withinTree, [expectedClass], requireNonEmpty);
    }

    private static void AssertAdornedClasses(
        Visual withinTree,
        IReadOnlyList<string> expectedClasses,
        bool requireNonEmpty = false)
    {
        var adornedElements = CollectAdornedElements(withinTree);
        if (expectedClasses.Count == 0)
        {
            // 该 Part 在当前模式下没有实例：断言目标数为 0，而不是要求存在高亮。
            adornedElements.ShouldBeEmpty();
            return;
        }

        if (requireNonEmpty)
        {
            adornedElements.ShouldNotBeEmpty();
        }

        foreach (var adornedElement in adornedElements)
        {
            var control = adornedElement.ShouldBeAssignableTo<Control>();
            expectedClasses.Any(expectedClass => control.Classes.Contains(expectedClass))
                           .ShouldBeTrue();
        }
    }

    private static Visual[] CollectAdornedElements(Visual withinTree)
    {
        var topLevel = TopLevel.GetTopLevel(withinTree).ShouldNotBeNull();
        return topLevel.GetVisualDescendants()
                       .OfType<AdornerLayer>()
                       .SelectMany(static layer => layer.Children)
                       .Where(static child => child.GetType().Name == "SemanticPartAdorner")
                       .Select(AdornerLayer.GetAdornedElement)
                       .OfType<Visual>()
                       .ToArray();
    }

    private static string[] SemanticPreviewPartPaths(SemanticPartPreview preview)
    {
        return SemanticPreviewItems(preview)
               .Select(static item => item.GetType().GetProperty("Path")?.GetValue(item) as string ?? string.Empty)
               .ToArray();
    }

    private static object SemanticPreviewPart(SemanticPartPreview preview, string path)
    {
        return SemanticPreviewItems(preview)
               .Single(item => Equals(
                   item.GetType().GetProperty("Path")?.GetValue(item) as string,
                   path));
    }

    private static int SemanticPreviewPartCount(SemanticPartPreview preview)
    {
        return SemanticPreviewItems(preview).Count();
    }

    private static IEnumerable<object> SemanticPreviewItems(SemanticPartPreview preview)
    {
        var itemsValue = typeof(SemanticPartPreview)
                         .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                         .ShouldNotBeNull()
                         .GetValue(preview);
        itemsValue.ShouldNotBeNull();
        return itemsValue.ShouldBeAssignableTo<IEnumerable>().Cast<object>();
    }

    private static void ShowInWindow(Control content, double width, double height, Action assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = content
        };
        // NavMenu 的 Vertical 弹层需要 OverlayPopupHost；该属性和其它预览测试一样按非公开入口打开。
        typeof(VisualLayerManager)
            .GetProperty("EnablePopupOverlayLayer", BindingFlags.Instance | BindingFlags.NonPublic)
            .ShouldNotBeNull()
            .SetValue(visualLayerManager, true);
        var window = new AtomUIWindow
        {
            Content = visualLayerManager,
            Width   = width,
            Height  = height
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static string ExtractMenuExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(StripNavMenuNodeCommandShowCaseItem(StripMenuRuntimeBindingMarkup(source)));
    }

    private static string StripNavMenuNodeCommandShowCaseItem(string source)
    {
        return Regex.Replace(
            source,
            @"\s*<gallery:ShowCaseItem\s+Title=""\{gallery:MenuShowCaseLangResource NavMenuNodeCommandTitle\}"".*?</gallery:ShowCaseItem>",
            string.Empty,
            RegexOptions.CultureInvariant | RegexOptions.Singleline);
    }

    private static string ExtractNavMenuNodeCommandShowCaseItem(string source)
    {
        return ExtractShowCaseItem(source, "NavMenuNodeCommandTitle");
    }

    private static string ExtractShowCaseItem(string source, string titleResourceName)
    {
        var match = Regex.Match(
            source,
            $@"<gallery:ShowCaseItem\s+Title=""\{{gallery:MenuShowCaseLangResource {titleResourceName}\}}"".*?</gallery:ShowCaseItem>",
            RegexOptions.CultureInvariant | RegexOptions.Singleline);

        match.Success.ShouldBeTrue();
        return match.Value;
    }

    private static string StripMenuRuntimeBindingMarkup(string source)
    {
        var normalized = Regex.Replace(
            source,
            @"\s+ItemsSource=""\{Binding (MenuItems|InlineNavMenuNodes|ItemsSourceDemoNavMenuNodes|ContextMenuItems)\}""",
            string.Empty,
            RegexOptions.CultureInvariant);

        return Regex.Replace(
            normalized,
            @"\s+(IsCheckedChanged=""HandleChange(Mode|Style)CheckChanged""|Click=""HandleToggleInlineCollapsedClick"")",
            string.Empty,
            RegexOptions.CultureInvariant);
    }

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

    private static string GetRepoFile(string relativePath)
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        return Path.Combine(repoRoot, relativePath);
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
