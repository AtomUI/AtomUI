using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using PopupConfirmControl = AtomUI.Desktop.Controls.PopupConfirm;

namespace AtomUI.Desktop.Controls.Tests.PopupConfirm;

public class PopupConfirmSemanticPartTests
{
    private const string PopupRootClass        = "semantic-popup-root";
    private const string PopupContainerClass   = "semantic-popup-container";
    private const string PopupContentClass     = "semantic-popup-content";
    private const string PopupArrowClass       = "semantic-popup-arrow";
    private const string PopupIconClass        = "semantic-popup-icon";
    private const string PopupTitleClass       = "semantic-popup-title";
    private const string PopupDescriptionClass = "semantic-popup-description";
    private const string PopupActionsClass     = "semantic-popup-actions";

    private const string PopupConfirmThemePath =
        "src/AtomUI.Desktop.Controls/PopupConfirm/Themes/PopupConfirmTheme.axaml";
    private const string PopupConfirmContainerThemePath =
        "src/AtomUI.Desktop.Controls/PopupConfirm/Themes/PopupConfirmContainerTheme.axaml";

    private static readonly string[] ApprovedPartNames =
    [
        "root",
        "popup.actions",
        "popup.arrow",
        "popup.container",
        "popup.content",
        "popup.description",
        "popup.icon",
        "popup.root",
        "popup.title"
    ];

    private static readonly string[] ContainerMarkerClasses =
    [
        PopupIconClass, PopupTitleClass, PopupDescriptionClass, PopupActionsClass
    ];

    private static readonly string[] AllSemanticMarkerClasses =
    [
        PopupRootClass, PopupContainerClass, PopupContentClass, PopupArrowClass,
        PopupIconClass, PopupTitleClass, PopupDescriptionClass, PopupActionsClass
    ];

    static PopupConfirmSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_PopupConfirm_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(PopupConfirmControl), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(PopupConfirmControl));

        // 弹层框体四部件复用 FlyoutHost 家族的 marker 类与跨视觉根路由。
        AssertPart(descriptor, "popup.root", PopupRootClass, typeof(FlyoutPresenter),
            ">> .semantic-popup-root");
        AssertPart(descriptor, "popup.container", PopupContainerClass, typeof(Border),
            ">> .semantic-popup-root >> .semantic-popup-container");
        AssertPart(descriptor, "popup.content", PopupContentClass, typeof(ContentPresenter),
            ">> .semantic-popup-root >> .semantic-popup-content");
        AssertPart(descriptor, "popup.arrow", PopupArrowClass, typeof(ArrowIndicator),
            ">> .semantic-popup-root >> .semantic-popup-arrow");

        // 确认体四部件 marker 静态声明在 PopupConfirmContainerTheme。
        AssertPart(descriptor, "popup.icon", PopupIconClass, typeof(IconPresenter),
            ">> .semantic-popup-root >> .semantic-popup-icon");
        AssertPart(descriptor, "popup.title", PopupTitleClass, typeof(Avalonia.Controls.TextBlock),
            ">> .semantic-popup-root >> .semantic-popup-title");
        AssertPart(descriptor, "popup.description", PopupDescriptionClass, typeof(ContentPresenter),
            ">> .semantic-popup-root >> .semantic-popup-description");
        AssertPart(descriptor, "popup.actions", PopupActionsClass, typeof(StackPanel),
            ">> .semantic-popup-root >> .semantic-popup-actions");

        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            part.CrossVisualRoot.ShouldBeTrue(
                $"{part.Name} must be cross-visual-root because the presenter is created in code outside the owner template");
            part.RuntimeCreated.ShouldBeTrue(
                $"{part.Name} must be runtime-created because its marker is injected outside the owner ControlTheme template");
        }
    }

    [Fact]
    public void Built_In_Templates_Implement_The_Approved_Static_Markers()
    {
        // 确认体四 marker 静态声明在 PopupConfirmContainer 自身主题。
        var containerDocument = XDocument.Load(
            GetRepoFile(PopupConfirmContainerThemePath), LoadOptions.SetLineInfo);
        var containerMarkers = containerDocument.Descendants()
            .SelectMany(static element => element.Attributes())
            .Where(static attribute => attribute.Name.LocalName.StartsWith("Classes.semantic-", StringComparison.Ordinal))
            .Select(static attribute =>
                $"{attribute.Name.LocalName}:{attribute.Parent!.Name.LocalName}")
            .OrderBy(static marker => marker, StringComparer.Ordinal)
            .ToArray();

        containerMarkers.ShouldBe([
            "Classes.semantic-popup-actions:StackPanel",
            "Classes.semantic-popup-description:ContentPresenter",
            "Classes.semantic-popup-icon:IconPresenter",
            "Classes.semantic-popup-title:TextBlock"
        ]);

        // owner 自身模板不声明任何弹层 marker（弹层由代码创建）。
        var ownerDocument = XDocument.Load(GetRepoFile(PopupConfirmThemePath), LoadOptions.SetLineInfo);
        ownerDocument.Descendants()
                     .SelectMany(static element => element.Attributes())
                     .Any(static attribute => attribute.Name.LocalName.StartsWith("Classes.semantic-", StringComparison.Ordinal))
                     .ShouldBeFalse();
    }

    [Fact]
    public void Default_Themes_Do_Not_Consume_Semantic_Selectors()
    {
        foreach (var path in new[] { PopupConfirmThemePath, PopupConfirmContainerThemePath })
        {
            var document = XDocument.Load(GetRepoFile(path), LoadOptions.SetLineInfo);
            var selectors = document.Descendants()
                                    .Where(static element => element.Name.LocalName == "Style")
                                    .Attributes("Selector")
                                    .Select(static attribute => attribute.Value)
                                    .ToArray();

            selectors.ShouldAllBe(static selector =>
                !selector.Contains("semantic-", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Are_Instantiable_For_Every_Part()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(PopupConfirmControl), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var styleType = part.StyleType.ShouldNotBeNull();
            Activator.CreateInstance(styleType).ShouldBeAssignableTo<Style>();
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Popup_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(PopupConfirmControl), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var anchor = new Border { Width = 100, Height = 30 };
        var host = new PopupConfirmControl
        {
            Content         = anchor,
            Title           = "Delete the task",
            ConfirmContent  = "Are you sure to delete this task?",
            IsMotionEnabled = false,
            ShouldUseOverlayPopup = true
        };
        host.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<PopupConfirmControl>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        host.Styles.Add(ownerStyle);

        var window = new AtomUIWindow
        {
            Width   = 480,
            Height  = 320,
            Content = host
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            host.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            host.Tag.ShouldBe("root");

            var flyout = host.Flyout.ShouldNotBeNull().ShouldBeOfType<PopupConfirmFlyout>();
            var popup = flyout.Popup.ShouldBeOfType<Popup>();
            popup.IsOpen.ShouldBeTrue();
            var presenter = popup.Child.ShouldBeOfType<FlyoutPresenter>();
            presenter.Classes.Contains(PopupRootClass).ShouldBeTrue();

            TagOf<FlyoutPresenter>(presenter, PopupRootClass).ShouldBe("popup.root");
            TagOf<Border>(presenter, PopupContainerClass).ShouldBe("popup.container");
            TagOf<ArrowIndicator>(presenter, PopupArrowClass).ShouldBe("popup.arrow");
            TagOf<IconPresenter>(presenter, PopupIconClass).ShouldBe("popup.icon");
            TagOf<Avalonia.Controls.TextBlock>(presenter, PopupTitleClass).ShouldBe("popup.title");
            TagOf<StackPanel>(presenter, PopupActionsClass).ShouldBe("popup.actions");
            TagOf<ContentPresenter>(presenter, PopupDescriptionClass).ShouldBe("popup.description");
            TagOf<ContentPresenter>(presenter, PopupContentClass).ShouldBe("popup.content");
        }
        finally
        {
            host.IsPopupPinnedOpen = false;
            host.Flyout?.Hide();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Title_Semantic_Style_Overrides_The_Default_Heading_Foreground()
    {
        // 回归：PopupConfirmContainerTheme 在 PART_Title 上静态设置 ColorTextHeading，
        // 因此仅靠 popup.root 的 Foreground 继承无法改变标题颜色（Style 优先级低于
        // Theme setter）。标题颜色必须由 PopupConfirmPopupTitleStyle 显式覆盖，
        // 与上游 PopupConfirm 的 title.color 行为一致。
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(PopupConfirmControl), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var host = new PopupConfirmControl
        {
            Content         = new Border { Width = 100, Height = 30 },
            Title           = "Function text",
            ConfirmContent  = "Function description",
            IsMotionEnabled = false,
            ShouldUseOverlayPopup = true
        };
        host.Classes.Add("semantic-foreground-owner");
        var ownerStyle = new Style(selector => selector.OfType<PopupConfirmControl>().Class("semantic-foreground-owner"));
        var rootStyle = (Style)Activator.CreateInstance(
            descriptor.Parts.Single(static part => part.Name == "popup.root").StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        rootStyle.Setters.Add(new Setter(TemplatedControl.ForegroundProperty, Brushes.White));
        ownerStyle.Children.Add(rootStyle);
        var titleStyle = (Style)Activator.CreateInstance(
            descriptor.Parts.Single(static part => part.Name == "popup.title").StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        titleStyle.Setters.Add(new Setter(Avalonia.Controls.TextBlock.ForegroundProperty, Brushes.White));
        ownerStyle.Children.Add(titleStyle);
        host.Styles.Add(ownerStyle);

        var window = new AtomUIWindow
        {
            Width   = 480,
            Height  = 320,
            Content = host
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            host.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            var presenter = host.Flyout.ShouldNotBeNull().ShouldBeOfType<PopupConfirmFlyout>()
                                .Popup.ShouldBeOfType<Popup>().Child.ShouldBeOfType<FlyoutPresenter>();
            var title = presenter.GetSelfAndVisualDescendants()
                                 .OfType<Avalonia.Controls.TextBlock>()
                                 .Single(control => control.Classes.Contains(PopupTitleClass));
            title.Foreground.ShouldBe(Brushes.White,
                "the generated PopupConfirmPopupTitleStyle must win over the ControlTheme heading color");
        }
        finally
        {
            host.IsPopupPinnedOpen = false;
            host.Flyout?.Hide();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static string? TagOf<T>(Visual root, string marker)
        where T : Control
    {
        return root.GetSelfAndVisualDescendants()
                   .OfType<T>()
                   .Single(control => control.Classes.Contains(marker))
                   .Tag as string;
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor, Type controlType)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(controlType);
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        string selectorRoute)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }
}
