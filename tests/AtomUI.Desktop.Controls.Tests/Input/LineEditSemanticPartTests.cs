using System.Xml.Linq;
using AtomUI.Controls;
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
using AtomUILineEdit = AtomUI.Desktop.Controls.LineEdit;
using AtomUISearchEdit = AtomUI.Desktop.Controls.SearchEdit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class LineEditSemanticPartTests
{
    private const string ClearClass = "semantic-clear";
    private const string CountClass = "semantic-count";
    private const string InputClass = "semantic-input";
    private const string PrefixClass = "semantic-prefix";
    private const string SuffixClass = "semantic-suffix";
    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/Input/Themes/LineEditTheme.axaml";

    static LineEditSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_LineEdit_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUILineEdit), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "clear", "count", "input", "prefix", "suffix"]);

        AssertRoot(descriptor);
        AssertPart(descriptor, "clear", ClearClass, typeof(AvaloniaButton),
            "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear");
        AssertPart(descriptor, "count", CountClass, typeof(TextBlock),
            "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-count");
        AssertPart(descriptor, "input", InputClass, typeof(TextPresenter),
            "/template/ .semantic-input");
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix");
        AssertPart(descriptor, "suffix", SuffixClass, typeof(StackPanel),
            "/template/ .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix");

        registry.TryGetControl(typeof(AtomUISearchEdit), out var searchDescriptor).ShouldBeTrue();
        searchDescriptor.ShouldNotBeNull();
        searchDescriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "button", "clear", "input", "prefix", "suffix"]);
    }

    [Fact]
    public void Built_In_Template_Implements_Only_The_Approved_Static_Markers()
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
                   "Classes.semantic-scope-input-frame:AddOnDecoratedBox",
                   "Classes.semantic-prefix:AddOnContentPresenter",
                   "Classes.semantic-suffix:StackPanel",
                   "Classes.semantic-clear:InputClearIconButton",
                   "Classes.semantic-count:TextBlock",
                   "Classes.semantic-content:Panel",
                   "Classes.semantic-placeholder:TextBlock",
                   "Classes.semantic-input:InputTextPresenter"
               ]);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();

        var literalMarkers = template.Descendants()
                                     .Attributes("Classes")
                                     .Where(static attribute => attribute.Value.Split(
                                         (char[]?)null,
                                         StringSplitOptions.RemoveEmptyEntries)
                                         .Any(static value => value.StartsWith(
                                             "semantic-",
                                             StringComparison.Ordinal)))
                                     .ToArray();
        literalMarkers.ShouldBeEmpty();

        var decoratedBoxTheme = XDocument.Load(
            GetRepoFile(
                "src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/AddOnDecoratedBoxTheme.axaml"),
            LoadOptions.SetLineInfo);
        var scopeMarkers = decoratedBoxTheme.Descendants()
                                                .SelectMany(static element => element.Attributes()
                                                    .Where(static attribute =>
                                                        attribute.Name.LocalName.StartsWith(
                                                            "Classes.semantic-scope-",
                                                            StringComparison.Ordinal))
                                                    .Select(attribute =>
                                                        $"{attribute.Name.LocalName}:{element.Name.LocalName}"))
                                                .ToArray();
        scopeMarkers.ShouldBe([
            "Classes.semantic-scope-prefix:AddOnContentPresenter",
            "Classes.semantic-scope-suffix:AddOnContentPresenter"
        ]);
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
    public void Generated_Semantic_Styles_Apply_To_The_LineEdit_Template_Targets()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUILineEdit), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var lineEdit = CreatePopulatedLineEdit();
        lineEdit.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUILineEdit>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        lineEdit.Styles.Add(ownerStyle);

        var window = Show(lineEdit);
        try
        {
            lineEdit.Tag.ShouldBe("root");
            FindSemanticControl<AvaloniaButton>(lineEdit, ClearClass).Tag.ShouldBe("clear");
            FindSemanticControl<TextBlock>(lineEdit, CountClass).Tag.ShouldBe("count");
            FindSemanticControl<TextPresenter>(lineEdit, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<ContentPresenter>(lineEdit, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<StackPanel>(lineEdit, SuffixClass).Tag.ShouldBe("suffix");
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(CustomizableSizeType.Large, 40d, 40d, 38d, 39d)]
    [InlineData(CustomizableSizeType.Middle, 32d, 32d, 30d, 31d)]
    [InlineData(CustomizableSizeType.Small, 22d, 22d, 20d, 21d)]
    public void Semantic_Markers_Preserve_The_Preset_Size_And_Variant_Baseline(
        CustomizableSizeType sizeType,
        double outlinedHeight,
        double filledHeight,
        double borderlessHeight,
        double underlinedHeight)
    {
        foreach (var variant in Enum.GetValues<InputControlStyleVariant>())
        {
            var lineEdit = CreatePopulatedLineEdit();
            lineEdit.SizeType = sizeType;
            lineEdit.StyleVariant = variant;

            var window = Show(lineEdit);
            try
            {
                var expectedHeight = variant switch
                {
                    InputControlStyleVariant.Outlined => outlinedHeight,
                    InputControlStyleVariant.Filled => filledHeight,
                    InputControlStyleVariant.Borderless => borderlessHeight,
                    InputControlStyleVariant.Underlined => underlinedHeight,
                    _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
                };
                lineEdit.Bounds.Height.ShouldBe(expectedHeight, 0.001);
                FindSemanticControl<ContentPresenter>(lineEdit, PrefixClass).Bounds.Height
                    .ShouldBeLessThanOrEqualTo(expectedHeight);
                FindSemanticControl<StackPanel>(lineEdit, SuffixClass).Bounds.Height
                    .ShouldBeLessThanOrEqualTo(expectedHeight);
            }
            finally
            {
                window.Close();
            }
        }
    }

    [Fact]
    public void Clear_Count_Prefix_And_Suffix_Markers_Remain_Stable_When_Content_State_Changes()
    {
        var lineEdit = CreatePopulatedLineEdit();

        var window = Show(lineEdit);
        try
        {
            var clear = FindSemanticControl<AvaloniaButton>(lineEdit, ClearClass);
            var count = FindSemanticControl<TextBlock>(lineEdit, CountClass);
            var prefix = FindSemanticControl<ContentPresenter>(lineEdit, PrefixClass);
            var suffix = FindSemanticControl<StackPanel>(lineEdit, SuffixClass);

            lineEdit.Text = null;
            lineEdit.IsShowCount = false;
            lineEdit.InnerLeftContent = null;
            lineEdit.InnerRightContent = null;
            Dispatcher.UIThread.RunJobs();

            FindSemanticControl<AvaloniaButton>(lineEdit, ClearClass).ShouldBeSameAs(clear);
            FindSemanticControl<TextBlock>(lineEdit, CountClass).ShouldBeSameAs(count);
            FindSemanticControl<ContentPresenter>(lineEdit, PrefixClass).ShouldBeSameAs(prefix);
            FindSemanticControl<StackPanel>(lineEdit, SuffixClass).ShouldBeSameAs(suffix);
            clear.IsEffectivelyVisible.ShouldBeFalse();
            count.IsEffectivelyVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUILineEdit CreatePopulatedLineEdit()
    {
        return new AtomUILineEdit
        {
            Width = 320,
            Text = "atomui",
            MaxLength = 12,
            IsAllowClear = true,
            IsShowCount = true,
            InnerLeftContent = "https://",
            InnerRightContent = ".com",
            IsMotionEnabled = false
        };
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(typeof(AtomUILineEdit));
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
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static T FindSemanticControl<T>(AtomUILineEdit owner, string marker)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(marker));
    }

    private static bool HasMarker(XElement element, string marker)
    {
        if (((string?)element.Attribute("Classes"))
            ?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Contains(marker, StringComparer.Ordinal) == true)
        {
            return true;
        }

        var classProperty = element.Attribute($"Classes.{marker}");
        return classProperty is not null &&
               bool.TryParse(classProperty.Value, out var enabled) &&
               enabled;
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
