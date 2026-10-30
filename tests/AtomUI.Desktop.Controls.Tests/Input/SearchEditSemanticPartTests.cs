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
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUISearchEdit = AtomUI.Desktop.Controls.SearchEdit;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class SearchEditSemanticPartTests
{
    private const string PrefixClass = "semantic-prefix";
    private const string InputClass = "semantic-input";
    private const string SuffixClass = "semantic-suffix";
    private const string ClearClass = "semantic-clear";
    private const string ButtonClass = "semantic-button";
    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/Input/Themes/SearchEditTheme.axaml";

    static SearchEditSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_The_Approved_SearchEdit_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUISearchEdit), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "button", "clear", "input", "prefix", "suffix"]);

        var buttonPart = descriptor.Parts.Single(static part => part.Name == "button");
        buttonPart.Path.ShouldBe("button");
        buttonPart.SelectorClass.ShouldBe(ButtonClass);
        buttonPart.SelectorRoute.ShouldBe("/template/ .semantic-scope-input-frame /template/ .semantic-button");
        buttonPart.ContractType.ShouldBe(typeof(AtomUIButton));
        buttonPart.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        buttonPart.Customization.ShouldBe(SemanticPartCustomization.Selector);
        buttonPart.RuntimeCreated.ShouldBeTrue();
        buttonPart.StyleType.ShouldNotBeNull();
        AssertPart(descriptor, "clear", ClearClass, typeof(Avalonia.Controls.Button),
            "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear");
        AssertPart(descriptor, "input", InputClass, typeof(TextPresenter),
            "/template/ .semantic-input");
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix");
        AssertPart(descriptor, "suffix", SuffixClass, typeof(StackPanel),
            "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix");

        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.SelectorClass.ShouldBeNull();
        root.StyleType.ShouldBeNull();
    }

    [Fact]
    public void Built_In_Template_Implements_The_Approved_Static_Markers()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);
        var template = document.Descendants()
                               .Single(static element => element.Name.LocalName == "ControlTemplate");
        var markers = template.Descendants()
                              .SelectMany(static element => element.Attributes()
                                  .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                      "Classes.semantic-",
                                      StringComparison.Ordinal))
                                  .Select(attribute => (Element: element, Attribute: attribute)))
                              .ToArray();

        markers.Select(static marker => $"{marker.Attribute.Name.LocalName}:{marker.Element.Name.LocalName}")
               .ShouldBe([
                   "Classes.semantic-scope-input-frame:SearchEditDecoratedBox",
                   "Classes.semantic-prefix:AddOnContentPresenter",
                   "Classes.semantic-suffix:StackPanel",
                   "Classes.semantic-clear:InputClearIconButton",
                   "Classes.semantic-content:Panel",
                   "Classes.semantic-placeholder:TextBlock",
                   "Classes.semantic-input:InputTextPresenter"
               ]);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));

        var buttonTheme = XDocument.Load(
            GetRepoFile("src/AtomUI.Desktop.Controls/Input/Themes/SearchEditDecoratedBoxTheme.axaml"),
            LoadOptions.SetLineInfo);
        var buttonMarkers = buttonTheme.Descendants()
                                       .SelectMany(static element => element.Attributes()
                                           .Where(static attribute =>
                                               attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                           .Select(attribute =>
                                               $"{attribute.Name.LocalName}:{element.Name.LocalName}"))
                                       .ToArray();

        buttonMarkers.ShouldBe([
            "Classes.semantic-scope-prefix:AddOnContentPresenter",
            "Classes.semantic-scope-suffix:AddOnContentPresenter"
        ]);
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_SearchEdit_Template_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUISearchEdit), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var searchEdit = new AtomUISearchEdit
        {
            Width = 320,
            PlaceholderText = "Search",
            IsAllowClear = true,
            InnerLeftContent = "https://",
            IsMotionEnabled = false
        };
        searchEdit.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUISearchEdit>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        searchEdit.Styles.Add(ownerStyle);

        var window = Show(searchEdit);
        try
        {
            searchEdit.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(searchEdit, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<TextPresenter>(searchEdit, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<StackPanel>(searchEdit, SuffixClass).Tag.ShouldBe("suffix");
            FindSemanticControl<Avalonia.Controls.Button>(searchEdit, ClearClass).Tag.ShouldBe("clear");
            FindSemanticControl<AtomUIButton>(searchEdit, ButtonClass).Tag.ShouldBe("button");
        }
        finally
        {
            window.Close();
        }
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
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static T FindSemanticControl<T>(AtomUISearchEdit owner, string marker)
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
