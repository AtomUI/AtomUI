using System.Reflection;
using System.Xml.Linq;
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
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIMentions = AtomUI.Desktop.Controls.Mentions;
using AtomUICandidateList = AtomUI.Desktop.Controls.Primitives.CandidateList;
using AtomUICandidateListItem = AtomUI.Desktop.Controls.Primitives.CandidateListItem;

namespace AtomUI.Desktop.Controls.Tests.Mention;

public class MentionsSemanticPartTests
{
    private const string PrefixClass = "semantic-prefix";
    private const string ContentClass = "semantic-content";
    private const string PlaceholderClass = "semantic-placeholder";
    private const string InputClass = "semantic-textarea";
    private const string ClearClass = "semantic-clear";
    private const string PopupRootClass = "semantic-popup-root";
    private const string PopupListClass = "semantic-popup-list";
    private const string PopupListItemClass = "semantic-popup-list-item";
    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/Mentions/Themes/MentionsTheme.axaml";

    private static readonly string[] ApprovedPartNames =
    [
        "root", "clear", "content", "input", "placeholder",
        "popup.list", "popup.listItem", "popup.root", "prefix"
    ];

    static MentionsSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Mentions_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIMentions), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIMentions));
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix");
        AssertPart(descriptor, "content", ContentClass, typeof(Panel),
            "/template/ .semantic-scope-input /template/ .semantic-content");
        AssertPart(descriptor, "placeholder", PlaceholderClass, typeof(TextBlock),
            "/template/ .semantic-scope-input /template/ .semantic-content > .semantic-placeholder");
        AssertPart(descriptor, "input", InputClass, typeof(TextPresenter),
            "/template/ .semantic-scope-input /template/ .semantic-textarea");
        AssertPart(descriptor, "clear", ClearClass, typeof(AvaloniaButton),
            "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear");
        AssertPart(descriptor, "popup.root", PopupRootClass, typeof(Border),
            "/template/ .semantic-popup-root");
        AssertPart(descriptor, "popup.list", PopupListClass, typeof(AtomUICandidateList),
            "/template/ .semantic-popup-root >> .semantic-popup-list",
            SemanticPartCardinality.Single);
        AssertPart(descriptor, "popup.listItem", PopupListItemClass, typeof(AtomUICandidateListItem),
            "/template/ .semantic-popup-list >> .semantic-popup-list-item",
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
                "Classes.semantic-scope-input:MentionTextArea",
                "Classes.semantic-popup-root:Border",
                "Classes.semantic-popup-list:CandidateList"
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
        registry.TryGetControl(typeof(AtomUIMentions), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var mentions = new AtomUIMentions
        {
            Width           = 320,
            IsAllowClear    = true,
            IsMotionEnabled = false
        };
        mentions.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIMentions>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root" &&
                                                                  !part.Name.StartsWith("popup.", StringComparison.Ordinal)))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        mentions.Styles.Add(ownerStyle);

        var window = Show(mentions);
        try
        {
            mentions.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(mentions, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<Panel>(mentions, ContentClass).Tag.ShouldBe("content");
            FindSemanticControl<TextBlock>(mentions, PlaceholderClass).Tag.ShouldBe("placeholder");
            FindSemanticControl<TextPresenter>(mentions, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<AvaloniaButton>(mentions, ClearClass).Tag.ShouldBe("clear");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Popup_Parts_Expose_Markers_When_Opened()
    {
        var mentions = new AtomUIMentions
        {
            Width             = 320,
            IsMotionEnabled   = false,
            IsPopupPinnedOpen = true,
            IsDropDownOpen    = true,
            OptionsSource =
            [
                new MentionOption { Header = "afc163", Value = "afc163" },
                new MentionOption { Header = "zombieJ", Value = "zombieJ" }
            ]
        };

        var window = CreateWindow(mentions);
        try
        {
            var popup = window.GetVisualDescendants().OfType<Popup>().Single();
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
        }
        finally
        {
            mentions.IsPopupPinnedOpen = false;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
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

    private static T FindSemanticControl<T>(AtomUIMentions owner, string marker)
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
            Width   = 480,
            Height  = 160,
            Content = content
        };
        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 640,
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
            Width   = 640,
            Height  = 480,
            Content = visualLayerManager
        };

        window.Show();
        content.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic);

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
