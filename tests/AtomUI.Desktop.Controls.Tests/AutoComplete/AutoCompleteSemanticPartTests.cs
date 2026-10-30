using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIAutoComplete = AtomUI.Desktop.Controls.AutoComplete;
using AtomUIAutoCompleteSearchEdit = AtomUI.Desktop.Controls.AutoCompleteSearchEdit;
using AtomUIAutoCompleteTextArea = AtomUI.Desktop.Controls.AutoCompleteTextArea;

namespace AtomUI.Desktop.Controls.Tests.AutoComplete;

public class AutoCompleteSemanticPartTests
{
    private const string ClearClass = "semantic-clear";
    private const string ContentClass = "semantic-content";
    private const string InputClass = "semantic-input";
    private const string PlaceholderClass = "semantic-placeholder";
    private const string PrefixClass = "semantic-prefix";
    private const string PopupRootClass = "semantic-popup-root";
    private const string PopupListClass = "semantic-popup-list";
    private const string PopupListItemClass = "semantic-popup-list-item";
    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/AutoComplete/Themes/AutoCompleteTheme.axaml";

    private static readonly string[] ApprovedPartNames =
    [
        "root", "clear", "content", "input", "placeholder",
        "popup.list", "popup.listItem", "popup.root", "prefix"
    ];

    static AutoCompleteSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_AutoComplete_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIAutoComplete), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(ApprovedPartNames);

        AssertRoot(descriptor, typeof(AtomUIAutoComplete));
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix");
        AssertPart(descriptor, "content", ContentClass, typeof(Panel),
            "/template/ .semantic-scope-input /template/ .semantic-content");
        AssertPart(descriptor, "placeholder", PlaceholderClass, typeof(TextBlock),
            "/template/ .semantic-scope-input /template/ .semantic-content > .semantic-placeholder");
        AssertPart(descriptor, "input", InputClass, typeof(TextPresenter),
            "/template/ .semantic-scope-input /template/ .semantic-input");
        AssertPart(descriptor, "clear", ClearClass, typeof(AvaloniaButton),
            "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear");
        AssertPart(descriptor, "popup.root", PopupRootClass, typeof(Border),
            "/template/ .semantic-popup-root");
        AssertPart(descriptor, "popup.list", PopupListClass, typeof(AtomUI.Desktop.Controls.Primitives.CandidateList),
            "/template/ .semantic-popup-root > .semantic-popup-list",
            SemanticPartCardinality.Single);
        AssertPart(descriptor, "popup.listItem", PopupListItemClass,
            typeof(AtomUI.Desktop.Controls.Primitives.CandidateListItem),
            "/template/ .semantic-popup-list >> .semantic-popup-list-item",
            SemanticPartCardinality.Multiple);

        registry.TryGetControl(typeof(AtomUIAutoCompleteSearchEdit), out var searchDescriptor).ShouldBeTrue();
        searchDescriptor.ShouldNotBeNull();
        searchDescriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);

        registry.TryGetControl(typeof(AtomUIAutoCompleteTextArea), out var textAreaDescriptor).ShouldBeTrue();
        textAreaDescriptor.ShouldNotBeNull();
        textAreaDescriptor.Parts.Select(static part => part.Name).ShouldBe(ApprovedPartNames);
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
                "Classes.semantic-scope-input:AutoCompleteLineEditBox",
                "Classes.semantic-popup-root:Border",
                "Classes.semantic-popup-list:CandidateList"
            ]);
            markers.ShouldAllBe(static marker => true);
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
        registry.TryGetControl(typeof(AtomUIAutoComplete), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var autoComplete = new AtomUIAutoComplete
        {
            Width = 320,
            IsAllowClear = true,
            IsMotionEnabled = false
        };
        autoComplete.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIAutoComplete>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root" &&
                                                                  !part.Name.StartsWith("popup.", StringComparison.Ordinal)))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        autoComplete.Styles.Add(ownerStyle);

        var window = Show(autoComplete);
        try
        {
            autoComplete.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(autoComplete, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<Panel>(autoComplete, ContentClass).Tag.ShouldBe("content");
            FindSemanticControl<TextBlock>(autoComplete, PlaceholderClass).Tag.ShouldBe("placeholder");
            FindSemanticControl<TextPresenter>(autoComplete, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<AvaloniaButton>(autoComplete, ClearClass).Tag.ShouldBe("clear");
        }
        finally
        {
            window.Close();
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

    private static T FindSemanticControl<T>(AtomUIAutoComplete owner, string marker)
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
