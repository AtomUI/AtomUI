using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIDropdownButton = AtomUI.Desktop.Controls.DropdownButton;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DropdownButtonControl;

public class DropdownButtonSemanticPartTests
{
    private const string DropdownButtonThemePath =
        "src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButtonTheme.axaml";
    private const string DropdownButtonBaseThemePath =
        "src/AtomUI.Desktop.Controls/Buttons/Themes/DropdownButtonBaseTheme.axaml";

    private static readonly string[] ApprovedPartNames =
    [
        "root", "item", "itemContent", "itemIcon", "itemTitle", "popup.root"
    ];

    static DropdownButtonSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_DropdownButton_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIDropdownButton), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIDropdownButton));
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
    public void Built_In_Templates_Carry_No_Trigger_Side_Static_Markers()
    {
        // antd 的 Dropdown Semantic DOM 全部位于弹层侧，触发器模板节点上不允许存在静态 semantic marker。
        var themeDocument = XDocument.Load(GetRepoFile(DropdownButtonThemePath), LoadOptions.SetLineInfo);
        var themeTemplates = themeDocument.Descendants()
            .Where(static element => element.Name.LocalName == "ControlTemplate").ToArray();
        themeTemplates.Length.ShouldBe(1);

        var baseDocument = XDocument.Load(GetRepoFile(DropdownButtonBaseThemePath), LoadOptions.SetLineInfo);
        var baseTemplates = baseDocument.Descendants()
            .Where(static element => element.Name.LocalName == "ControlTemplate").ToArray();
        baseTemplates.Length.ShouldBe(3);

        foreach (var template in themeTemplates.Concat(baseTemplates))
        {
            CollectMarkers(template).ShouldBeEmpty();
        }
    }

    [Fact]
    public void Popup_Parts_Expose_Markers_When_Opened()
    {
        var dropdownButton = CreatePinnedDropdownButton();

        ShowInWindow(dropdownButton, window =>
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
            // the "Paste" submenu items are only materialized once the submenu opens,
            // so open it and require the nested items to carry the popup item marker too.
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
    public void Generated_Popup_Semantic_Styles_Apply_To_The_Popup_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUIDropdownButton), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var dropdownButton = CreatePinnedDropdownButton();
        dropdownButton.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIDropdownButton>().Class("semantic-owner"));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        dropdownButton.Styles.Add(ownerStyle);

        ShowInWindow(dropdownButton, window =>
        {
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
                  .First(control => control.Classes.Contains("semantic-item-title"))
                  .Tag.ShouldBe("itemTitle");
        });
    }

    [Fact]
    public void Pinned_Open_Disables_Light_Dismiss_On_The_Flyout_Popup()
    {
        var dropdownButton = CreatePinnedDropdownButton();

        ShowInWindow(dropdownButton, window =>
        {
            var popup = dropdownButton.DropdownFlyout.ShouldNotBeNull().Popup;
            popup.ShouldNotBeNull();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse(
                "a pinned preview popup must not leave a light-dismiss overlay blocking the rest of the window");
        });
    }

    [Fact]
    public void Pinned_Popup_With_Declaratively_Open_Submenu_Does_Not_Crash_On_Initial_Open()
    {
        var dropdownButton = CreatePinnedDropdownButton();
        var pasteParent = dropdownButton.DropdownFlyout.ShouldNotBeNull().Items
                                        .OfType<AtomUI.Desktop.Controls.MenuItem>()
                                        .First(item => Equals(item.Header, "Paste") && item.Items.Count > 0);
        // 声明式打开（对应 Gallery SemanticPartPreview 的 IsSubMenuOpen="True"，antd defaultOpenKeys）。
        pasteParent.IsSubMenuOpen = true;

        ShowInWindow(dropdownButton, window =>
        {
            dropdownButton.DropdownFlyout.Popup.ShouldNotBeNull().IsOpen.ShouldBeTrue();
            pasteParent.IsSubMenuOpen.ShouldBeTrue();
            window.GetVisualDescendants()
                  .OfType<AtomUI.Desktop.Controls.MenuItem>()
                  .First(item => Equals(item.Header, "Paste from history"))
                  .ShouldNotBeNull();
        });
    }

    private static AtomUIDropdownButton CreatePinnedDropdownButton()
    {
        return new AtomUIDropdownButton
        {
            Content = "Actions",
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true,
            DropdownFlyout = new AtomUI.Desktop.Controls.MenuFlyout
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
            }
        };
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

    private static string[] CollectMarkers(XElement template)
    {
        return template.Descendants()
                       .SelectMany(static element => element.Attributes()
                           .Where(static attribute => attribute.Name.LocalName.StartsWith(
                               "Classes.semantic-", StringComparison.Ordinal)))
                       .Select(static attribute =>
                           $"{attribute.Name.LocalName["Classes.".Length..]}:{attribute.Parent!.Name.LocalName}")
                       .ToArray();
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
