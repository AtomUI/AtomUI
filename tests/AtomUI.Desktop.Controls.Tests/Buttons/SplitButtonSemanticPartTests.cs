using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISplitButton = AtomUI.Desktop.Controls.SplitButton;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

public class SplitButtonSemanticPartTests
{
    private const string SplitButtonThemePath =
        "src/AtomUI.Desktop.Controls/Buttons/Themes/SplitButtonTheme.axaml";

    private static readonly string[] ApprovedPartNames =
    [
        "root", "item", "itemContent", "itemIcon", "itemTitle", "popup.root", "primary", "secondary"
    ];

    static SplitButtonSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_SplitButton_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUISplitButton), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUISplitButton));
        AssertPart(descriptor, "primary", "semantic-primary",
            typeof(AtomUI.Desktop.Controls.Button), "/template/ .semantic-primary");
        AssertPart(descriptor, "secondary", "semantic-secondary",
            typeof(AtomUI.Desktop.Controls.Button), "/template/ .semantic-secondary");
        AssertPart(descriptor, "popup.root", "semantic-popup-root",
            typeof(AtomUI.Desktop.Controls.ArrowDecoratedBox), ">> .semantic-popup-root");
        AssertPart(descriptor, "item", "semantic-item",
            typeof(AtomUI.Desktop.Controls.MenuItem), ">> .semantic-item",
            SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "itemIcon", "semantic-item-icon", typeof(IconPresenter),
            ">> .semantic-item /template/ .semantic-item-icon");
        AssertPart(descriptor, "itemContent", "semantic-item-content", typeof(ContentPresenter),
            ">> .semantic-item /template/ .semantic-item-content");
        AssertPart(descriptor, "itemTitle", "semantic-item-title", typeof(ContentPresenter),
            ">> .semantic-item-title-group /template/ .semantic-item-title",
            SemanticPartCardinality.Multiple);
    }

    [Fact]
    public void Built_In_Template_Carries_Trigger_Side_Static_Markers()
    {
        // 触发侧 primary / secondary 是静态 marker：SplitButtonTheme.axaml 的唯一内置模板中
        // PART_PrimaryButton / PART_SecondaryButton 必须分别携带 semantic-primary / semantic-secondary。
        var themeDocument = XDocument.Load(GetRepoFile(SplitButtonThemePath), LoadOptions.SetLineInfo);
        var themeTemplates = themeDocument.Descendants()
            .Where(static element => element.Name.LocalName == "ControlTemplate").ToArray();
        themeTemplates.Length.ShouldBe(1);

        var primaryButton = themeTemplates[0].Descendants()
            .Single(static element => (string?)element.Attribute("Name") == "PART_PrimaryButton");
        primaryButton.Attribute("Classes.semantic-primary").ShouldNotBeNull();

        var secondaryButton = themeTemplates[0].Descendants()
            .Single(static element => (string?)element.Attribute("Name") == "PART_SecondaryButton");
        secondaryButton.Attribute("Classes.semantic-secondary").ShouldNotBeNull();
    }

    [Fact]
    public void Trigger_Side_Markers_Are_Present_After_Template_Apply()
    {
        var splitButton = CreateSplitButton();

        ShowInWindow(splitButton, window =>
        {
            var primaryButton = window.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.Button>()
                                      .Single(button => button.Name == "PART_PrimaryButton");
            primaryButton.Classes.Contains("semantic-primary").ShouldBeTrue();

            var secondaryButton = window.GetVisualDescendants()
                                        .OfType<AtomUI.Desktop.Controls.Button>()
                                        .Single(button => button.Name == "PART_SecondaryButton");
            secondaryButton.Classes.Contains("semantic-secondary").ShouldBeTrue();
        });
    }

    [Fact]
    public void Popup_Parts_Expose_Markers_When_Pinned_Open()
    {
        var splitButton = CreatePinnedSplitButton();

        ShowInWindow(splitButton, window =>
        {
            window.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.ArrowDecoratedBox>()
                  .Single(control => control.Classes.Contains("semantic-popup-root"))
                  .ShouldNotBeNull();

            window.GetVisualDescendants()
                  .OfType<Control>()
                  .Count(control => control.Classes.Contains("semantic-item"))
                  .ShouldBeGreaterThanOrEqualTo(3);

            // Realize the nested submenu so the MenuItem container path is exercised:
            // the "Paste" submenu items are only materialized once the submenu opens.
            var pasteParent = window.GetVisualDescendants()
                                    .OfType<AtomUI.Desktop.Controls.MenuItem>()
                                    .First(item => Equals(item.Header, "Paste") && item.Items.Count > 0);
            pasteParent.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            window.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.MenuItem>()
                  .First(item => Equals(item.Header, "Paste from history"))
                  .Classes.Contains("semantic-item")
                  .ShouldBeTrue();

            window.GetVisualDescendants()
                  .OfType<Control>()
                  .Count(control => control.Classes.Contains("semantic-item"))
                  .ShouldBeGreaterThanOrEqualTo(5);

            window.GetVisualDescendants()
                  .OfType<IconPresenter>()
                  .Any(control => control.Classes.Contains("semantic-item-icon"))
                  .ShouldBeTrue();
            window.GetVisualDescendants()
                  .OfType<ContentPresenter>()
                  .Any(control => control.Classes.Contains("semantic-item-content"))
                  .ShouldBeTrue();
            window.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.MenuItemGroup>()
                  .Single(control => control.Classes.Contains("semantic-item-title-group"))
                  .ShouldNotBeNull();
            window.GetVisualDescendants()
                  .OfType<ContentPresenter>()
                  .Single(control => control.Classes.Contains("semantic-item-title"))
                  .ShouldNotBeNull();
        });
    }

    [Fact]
    public void Pinned_Open_Disables_Light_Dismiss_On_The_Flyout_Popup()
    {
        var splitButton = CreatePinnedSplitButton();

        ShowInWindow(splitButton, window =>
        {
            var flyout = splitButton.Flyout.ShouldNotBeNull();
            var popup = flyout.Popup;
            popup.ShouldNotBeNull();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse(
                "a pinned preview popup must not leave a light-dismiss overlay blocking the rest of the window");
        });
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_Trigger_And_Popup_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUISplitButton), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var splitButton = CreatePinnedSplitButton();
        splitButton.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUISplitButton>().Class("semantic-owner"));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        splitButton.Styles.Add(ownerStyle);

        ShowInWindow(splitButton, window =>
        {
            window.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.Button>()
                  .Single(button => button.Name == "PART_PrimaryButton")
                  .Tag.ShouldBe("primary");
            window.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.Button>()
                  .Single(button => button.Name == "PART_SecondaryButton")
                  .Tag.ShouldBe("secondary");
            window.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.ArrowDecoratedBox>()
                  .Single(control => control.Classes.Contains("semantic-popup-root"))
                  .Tag.ShouldBe("popup.root");
            window.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.MenuItem>()
                  .First(control => control.Classes.Contains("semantic-item"))
                  .Tag.ShouldBe("item");
            window.GetVisualDescendants()
                  .OfType<IconPresenter>()
                  .First(control => control.Classes.Contains("semantic-item-icon"))
                  .Tag.ShouldBe("itemIcon");
            window.GetVisualDescendants()
                  .OfType<ContentPresenter>()
                  .First(control => control.Classes.Contains("semantic-item-content"))
                  .Tag.ShouldBe("itemContent");
            window.GetVisualDescendants()
                  .OfType<ContentPresenter>()
                  .Single(control => control.Classes.Contains("semantic-item-title"))
                  .Tag.ShouldBe("itemTitle");
        });
    }

    [Theory]
    [InlineData(false, SharedTokenKind.ColorPrimaryHover)]
    [InlineData(true, SharedTokenKind.ColorErrorHover)]
    public void Primary_Shape_Separator_Color_Follows_Danger_Hover_Token(bool isDanger, SharedTokenKind expectedToken)
    {
        // 对齐上游 solid 组合规则：primary 形态接缝分隔线颜色取 colorPrimaryHover /
        // colorErrorHover（由 IsDanger 决定），不再取 ColorBorder。
        var splitButton = new AtomUISplitButton
        {
            Content = "Split Action",
            IsPrimaryButtonType = true,
            IsDanger = isDanger,
            IsMotionEnabled = false,
            Flyout = CreateMenuFlyout()
        };

        ShowInWindow(splitButton, window =>
        {
            var expectedColor = GetSolidBrushColor(GetThemeResource<IBrush>(expectedToken));
            var separatorBrush = splitButton.SplitSeparatorBrush.ShouldNotBeNull();
            GetSolidBrushColor(separatorBrush).ShouldBe(expectedColor);
        });
    }

    [Fact]
    public void Default_Shape_Separator_Brush_Keeps_ColorBorder()
    {
        // default（outline）形态接缝无边框线，分隔线画刷保持 ColorBorder 语义。
        var splitButton = new AtomUISplitButton
        {
            Content = "Split Action",
            IsMotionEnabled = false,
            Flyout = CreateMenuFlyout()
        };

        ShowInWindow(splitButton, window =>
        {
            var expectedColor = GetSolidBrushColor(GetThemeResource<IBrush>(SharedTokenKind.ColorBorder));
            var separatorBrush = splitButton.SplitSeparatorBrush.ShouldNotBeNull();
            GetSolidBrushColor(separatorBrush).ShouldBe(expectedColor);
        });
    }

    [Fact]
    public void Primary_Shape_Separator_Hides_When_Secondary_Button_Is_Pointed_Over()
    {
        // 对齐上游 solid 组合规则：带分隔线的次按钮 hover 时隐藏分隔线。
        var splitButton = new AtomUISplitButton
        {
            Width = 160,
            Height = 36,
            Content = "Split Action",
            IsPrimaryButtonType = true,
            IsMotionEnabled = false,
            Flyout = CreateMenuFlyout()
        };

        ShowInWindow(splitButton, window =>
        {
            var secondaryButton = window.GetVisualDescendants()
                                        .OfType<AtomUI.Desktop.Controls.Button>()
                                        .Single(button => button.Name == "PART_SecondaryButton");

            var separatorVisibleBefore = splitButton.IsSeparatorVisible;
            separatorVisibleBefore.ShouldBeTrue();

            RaisePointerEntered(secondaryButton, window);
            Dispatcher.UIThread.RunJobs();

            splitButton.IsSeparatorVisible.ShouldBeFalse();

            RaisePointerExited(secondaryButton, window);
            Dispatcher.UIThread.RunJobs();

            splitButton.IsSeparatorVisible.ShouldBeTrue();
        });
    }

    private static AtomUISplitButton CreateSplitButton()
    {
        return new AtomUISplitButton
        {
            Content = "Split Action",
            IsMotionEnabled = false,
            Flyout = CreateMenuFlyout()
        };
    }

    private static AtomUISplitButton CreatePinnedSplitButton()
    {
        return new AtomUISplitButton
        {
            Content = "Split Action",
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true,
            Flyout = CreateMenuFlyout()
        };
    }

    private static MenuFlyout CreateMenuFlyout()
    {
        return new MenuFlyout
        {
            Items =
            {
                new AtomUI.Desktop.Controls.MenuItemGroup
                {
                    Header = "Group title",
                    Items =
                    {
                        new AtomUI.Desktop.Controls.MenuItem { Header = "1st menu item" },
                        new AtomUI.Desktop.Controls.MenuItem { Header = "2nd menu item" }
                    }
                },
                new AtomUI.Desktop.Controls.MenuItem { Header = "Cut" },
                new AtomUI.Desktop.Controls.MenuItem { Header = "Copy" },
                new AtomUI.Desktop.Controls.MenuItem
                {
                    Header = "Paste",
                    Items =
                    {
                        new AtomUI.Desktop.Controls.MenuItem { Header = "Paste" },
                        new AtomUI.Desktop.Controls.MenuItem { Header = "Paste from history" }
                    }
                }
            }
        };
    }

    private static void RaisePointerEntered(InputElement target, AvaloniaWindow window)
    {
        target.RaiseEvent(new PointerEventArgs(
            InputElement.PointerEnteredEvent,
            target,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            target,
            target.Bounds.Center,
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            KeyModifiers.None));
    }

    private static void RaisePointerExited(InputElement target, AvaloniaWindow window)
    {
        target.RaiseEvent(new PointerEventArgs(
            InputElement.PointerExitedEvent,
            target,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            target,
            target.Bounds.Center,
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            KeyModifiers.None));
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static Color GetSolidBrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
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
        string selectorRoute,
        SemanticPartCardinality cardinality = SemanticPartCardinality.Single)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width = 640,
            Height = 480
        };
        overlayPanel.Children.Add(content);
        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width = 640,
            Height = 480,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
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
