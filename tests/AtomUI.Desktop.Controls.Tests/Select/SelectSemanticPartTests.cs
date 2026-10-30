using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISelect = AtomUI.Desktop.Controls.Select;

namespace AtomUI.Desktop.Controls.Tests.SelectControl;

public class SelectSemanticPartTests
{
    private const string PrefixClass = "semantic-prefix";
    private const string ContentClass = "semantic-content";
    private const string PlaceholderClass = "semantic-placeholder";
    private const string InputClass = "semantic-input";
    private const string SuffixClass = "semantic-suffix";
    private const string ClearClass = "semantic-clear";
    private const string PopupRootClass = "semantic-popup-root";
    private const string PopupListClass = "semantic-popup-list";
    private const string ItemClass = "semantic-item";
    private const string ItemContentClass = "semantic-item-content";
    private const string ItemRemoveClass = "semantic-item-remove";
    private const string PopupListItemClass = "semantic-popup-list-item";
    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/Select/Themes/SelectTheme.axaml";

    private static readonly string[] ApprovedPartNames =
    [
        "root", "clear", "content", "input", "item", "itemContent", "itemRemove",
        "placeholder", "popup.list", "popup.listItem", "popup.root", "prefix", "suffix"
    ];

    static SelectSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Select_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUISelect), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUISelect));
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix");
        AssertPart(descriptor, "content", ContentClass, typeof(Panel),
            "/template/ .semantic-content");
        AssertPart(descriptor, "item", ItemClass, typeof(AtomUI.Desktop.Controls.Tag),
            ">> .semantic-scope-tags >> .semantic-item", SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "itemContent", ItemContentClass, typeof(TextBlock),
            ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-content");
        AssertPart(descriptor, "itemRemove", ItemRemoveClass, typeof(IconButton),
            ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-remove");
        AssertPart(descriptor, "placeholder", PlaceholderClass, typeof(TextBlock),
            "/template/ .semantic-content > .semantic-placeholder");
        AssertPart(descriptor, "input", InputClass, typeof(TextBox),
            "/template/ .semantic-content > .semantic-input");
        AssertPart(descriptor, "suffix", SuffixClass, typeof(StackPanel),
            "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix");
        AssertPart(descriptor, "clear", ClearClass, typeof(AvaloniaButton),
            ">> .semantic-scope-handle /template/ .semantic-clear");
        AssertPart(descriptor, "popup.root", PopupRootClass, typeof(Border),
            "/template/ .semantic-popup-root");
        AssertPart(descriptor, "popup.list", PopupListClass, typeof(Control),
            "/template/ .semantic-popup-root >> .semantic-popup-list",
            SemanticPartCardinality.Single);
        AssertPart(descriptor, "popup.listItem", PopupListItemClass, typeof(TemplatedControl),
            "/template/ .semantic-popup-root >> .semantic-popup-list-item",
            SemanticPartCardinality.Multiple);
    }

    [Fact]
    public void Built_In_Templates_Implement_The_Approved_Static_Markers()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);
        var templates = document.Descendants()
                                .Where(static element => element.Name.LocalName == "ControlTemplate")
                                .ToArray();
        templates.Length.ShouldBe(1);

        foreach (var template in templates)
        {
            var markers = template.Descendants()
                                  .SelectMany(static element => element.Attributes()
                                      .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                          "Classes.semantic-",
                                          StringComparison.Ordinal)))
                                  .Select(static attribute => $"{attribute.Name.LocalName}:{attribute.Parent!.Name.LocalName}")
                                  .ToArray();
            markers.ShouldBe([
                "Classes.semantic-scope-input:SelectAddOnDecoratedBox",
                "Classes.semantic-prefix:AddOnContentPresenter",
                "Classes.semantic-suffix:StackPanel",
                "Classes.semantic-scope-handle:SelectHandle",
                "Classes.semantic-content:Panel",
                "Classes.semantic-placeholder:TextBlock",
                "Classes.semantic-input:SelectFilterTextBox",
                "Classes.semantic-scope-tags:SelectResultOptionsBox",
                "Classes.semantic-popup-root:Border"
            ]);
        }

        document.Descendants()
                .Any(static element => element.Attributes()
                    .Any(static attribute => attribute.Name.LocalName == "Classes.semantic-root"))
                .ShouldBeFalse();
    }

    [Fact]
    public void Default_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);
        var selectors = document.Descendants()
                                .Where(static element => element.Name.LocalName == "Style")
                                .Attributes("Selector")
                                .Select(static attribute => attribute.Value)
                                .ToArray();

        selectors.ShouldAllBe(static selector =>
            !selector.Contains("semantic-", StringComparison.Ordinal));
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Template_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUISelect), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var select = new AtomUISelect
        {
            Width = 320,
            IsAllowClear = true,
            IsMotionEnabled = false
        };
        select.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUISelect>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root" &&
                                                                  !part.Name.StartsWith("popup.", StringComparison.Ordinal)))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        select.Styles.Add(ownerStyle);

        var window = Show(select);
        try
        {
            select.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(select, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<Panel>(select, ContentClass).Tag.ShouldBe("content");
            FindSemanticControl<TextBlock>(select, PlaceholderClass).Tag.ShouldBe("placeholder");
            FindSemanticControl<TextBox>(select, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<StackPanel>(select, SuffixClass).Tag.ShouldBe("suffix");
            FindSemanticControl<AvaloniaButton>(select, ClearClass).Tag.ShouldBe("clear");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Tag_Part_Styles_Apply_To_Multiple_Selection_Tags()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUISelect), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var select = new AtomUISelect
        {
            Width = 420,
            Mode = SelectMode.Multiple,
            IsMotionEnabled = false,
            OptionsSource = new List<ISelectOption>
            {
                new SelectOption { Header = "GuangZhou", Content = "GuangZhou" },
                new SelectOption { Header = "ShenZhen", Content = "ShenZhen" }
            }
        };
        select.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUISelect>().Class("semantic-owner"));
        foreach (var partName in new[] { "item", "itemContent", "itemRemove" })
        {
            var part = descriptor.Parts.Single(candidate => candidate.Name == partName);
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            var capturedName = partName;
            partStyle.Setters.Add(new Setter(Control.TagProperty, capturedName));
            ownerStyle.Children.Add(partStyle);
        }
        select.Styles.Add(ownerStyle);

        var window = Show(select);
        try
        {
            var first = select.OptionsSource!.First();
            select.SelectedOptions = new List<ISelectOption> { first };
            Dispatcher.UIThread.RunJobs();

            var tag = select.GetVisualDescendants()
                            .OfType<Control>()
                            .FirstOrDefault(control => control.Classes.Contains(ItemClass));
            tag.ShouldNotBeNull();
            select.GetVisualDescendants()
                  .OfType<TextBlock>()
                  .First(control => control.Classes.Contains(ItemContentClass))
                  .Tag.ShouldBe("itemContent");
            select.GetVisualDescendants()
                  .OfType<IconButton>()
                  .First(control => control.Classes.Contains(ItemRemoveClass))
                  .Tag.ShouldBe("itemRemove");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Multiple_Mode_Search_Input_Carries_The_Input_Marker()
    {
        var select = new AtomUISelect
        {
            Width = 420,
            Mode = SelectMode.Multiple,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            OptionsSource = new List<ISelectOption>
            {
                new SelectOption { Header = "GuangZhou", Content = "GuangZhou" },
                new SelectOption { Header = "ShenZhen", Content = "ShenZhen" }
            }
        };

        var window = Show(select);
        try
        {
            var searchInput = select.GetVisualDescendants()
                                    .OfType<TextBox>()
                                    .Single(control => control.Classes.Contains(InputClass) &&
                                                       control.IsEffectivelyVisible);
            searchInput.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Prefix_Part_Presents_The_ContentLeftAddOn_Inline_In_The_Frame()
    {
        var select = new AtomUISelect
        {
            Width = 320,
            IsMotionEnabled = false,
            ContentLeftAddOn = "prefix"
        };

        var window = Show(select);
        try
        {
            var prefix = select.GetVisualDescendants()
                               .OfType<ContentPresenter>()
                               .Single(control => control.Classes.Contains(PrefixClass));
            prefix.IsVisible.ShouldBeTrue();
            prefix.Content.ShouldBe("prefix");
            prefix.Bounds.Width.ShouldBeGreaterThan(0);
            prefix.Bounds.Height.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Popup_Parts_Expose_Markers_When_Opened()
    {
        var select = new AtomUISelect
        {
            Width = 320,
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true,
            IsDropDownOpen = true,
            OptionsSource = new List<ISelectOption>
            {
                new SelectOption { Header = "GuangZhou", Content = "GuangZhou" },
                new SelectOption { Header = "ShenZhen", Content = "ShenZhen" }
            }
        };

        ShowInWindow(select, window =>
        {
            var popup = select.GetVisualDescendants().OfType<Popup>().Single();
            popup.IsOpen.ShouldBeTrue();

            var lists = window.GetVisualDescendants()
                              .OfType<Control>()
                              .Where(control => control.Classes.Contains(PopupListClass))
                              .ToArray();
            lists.Length.ShouldBeGreaterThanOrEqualTo(1);

            window.GetVisualDescendants()
                  .OfType<Control>()
                  .Count(control => control.Classes.Contains(PopupListItemClass))
                  .ShouldBeGreaterThanOrEqualTo(2);
        });
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

    private static T FindSemanticControl<T>(AtomUISelect owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(marker));
    }

    private static AvaloniaWindow Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 160,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
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
