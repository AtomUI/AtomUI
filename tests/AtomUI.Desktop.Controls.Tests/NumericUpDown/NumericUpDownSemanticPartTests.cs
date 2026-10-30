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
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NumericUpDown;

public class NumericUpDownSemanticPartTests
{
    private const string ClearClass = "semantic-clear";
    private const string InputClass = "semantic-input";
    private const string PrefixClass = "semantic-prefix";
    private const string SuffixClass = "semantic-suffix";
    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/NumericUpDown/Themes/NumericUpDownTheme.axaml";

    static NumericUpDownSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_NumericUpDown_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUINumericUpDown), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "clear", "input", "prefix", "suffix"]);

        AssertRoot(descriptor);
        AssertPart(descriptor, "clear", ClearClass, typeof(AvaloniaButton),
            "/template/ .semantic-clear");
        AssertPart(descriptor, "input", InputClass, typeof(TextBox),
            "/template/ .semantic-input");
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter),
            "/template/ .semantic-scope-spinner >> .semantic-prefix");
        AssertPart(descriptor, "suffix", SuffixClass, typeof(StackPanel),
            "/template/ .semantic-suffix");
    }

    [Fact]
    public void Built_In_Templates_Implement_The_Approved_Static_Markers()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);
        var templates = document.Descendants()
                                .Where(static element => element.Name.LocalName == "ControlTemplate")
                                .ToArray();
        templates.Length.ShouldBe(2);

        foreach (var template in templates)
        {
            var markers = template.Descendants()
                                  .SelectMany(static element => element.Attributes()
                                      .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                          "Classes.semantic-",
                                          StringComparison.Ordinal))
                                      .Select(attribute => (Element: element, Attribute: attribute)))
                                  .ToArray();
            markers.Select(static marker => $"{marker.Attribute.Name.LocalName}:{marker.Element.Name.LocalName}")
                   .ShouldBe([
                       "Classes.semantic-scope-spinner:NumericUpDownSpinner",
                       "Classes.semantic-prefix:AddOnContentPresenter",
                       "Classes.semantic-suffix:StackPanel",
                       "Classes.semantic-clear:InputClearIconButton",
                       "Classes.semantic-input:EmbeddedTextBox"
                   ]);
            markers.ShouldAllBe(static marker =>
                string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
            template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();
        }

        var literalMarkers = document.Descendants()
                                     .Attributes("Classes")
                                     .Where(static attribute => attribute.Value.Split(
                                         (char[]?)null,
                                         StringSplitOptions.RemoveEmptyEntries)
                                         .Any(static value => value.StartsWith(
                                             "semantic-",
                                             StringComparison.Ordinal)))
                                     .ToArray();
        literalMarkers.ShouldBeEmpty();

        foreach (var frameThemePath in new[]
                 {
                     "src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerTheme.axaml",
                     "src/AtomUI.Desktop.Controls/NumericUpDown/Themes/NumericUpDownSpinnerTheme.axaml"
                 })
        {
            var frameTheme = XDocument.Load(GetRepoFile(frameThemePath), LoadOptions.SetLineInfo);
            frameTheme.Descendants()
                      .Count(static element => HasMarker(element, "semantic-scope-frame"))
                      .ShouldBe(1);
        }
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

    [Theory]
    [InlineData(NumericUpDownMode.Input)]
    [InlineData(NumericUpDownMode.Spinner)]
    public void Generated_Semantic_Styles_Apply_To_The_Template_Targets(NumericUpDownMode mode)
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;
        registry.TryGetControl(typeof(AtomUINumericUpDown), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var numericUpDown = CreatePopulatedNumericUpDown();
        numericUpDown.Mode = mode;
        numericUpDown.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUINumericUpDown>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        numericUpDown.Styles.Add(ownerStyle);

        var window = Show(numericUpDown);
        try
        {
            numericUpDown.Tag.ShouldBe("root");
            FindSemanticControl<ContentPresenter>(numericUpDown, PrefixClass).Tag.ShouldBe("prefix");
            FindSemanticControl<TextBox>(numericUpDown, InputClass).Tag.ShouldBe("input");
            FindSemanticControl<StackPanel>(numericUpDown, SuffixClass).Tag.ShouldBe("suffix");
            FindSemanticControl<AvaloniaButton>(numericUpDown, ClearClass).Tag.ShouldBe("clear");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Markers_Remain_Stable_When_Content_State_Changes()
    {
        var numericUpDown = CreatePopulatedNumericUpDown();

        var window = Show(numericUpDown);
        try
        {
            var clear = FindSemanticControl<AvaloniaButton>(numericUpDown, ClearClass);
            var prefix = FindSemanticControl<ContentPresenter>(numericUpDown, PrefixClass);
            var suffix = FindSemanticControl<StackPanel>(numericUpDown, SuffixClass);

            numericUpDown.Value = null;
            numericUpDown.InnerLeftContent = null;
            numericUpDown.InnerRightContent = null;
            Dispatcher.UIThread.RunJobs();

            FindSemanticControl<AvaloniaButton>(numericUpDown, ClearClass).ShouldBeSameAs(clear);
            FindSemanticControl<ContentPresenter>(numericUpDown, PrefixClass).ShouldBeSameAs(prefix);
            FindSemanticControl<StackPanel>(numericUpDown, SuffixClass).ShouldBeSameAs(suffix);
            clear.IsEffectivelyVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Clear_Button_Stays_Vertically_Centered_When_Suffix_Content_Is_Taller()
    {
        var stackPanel = new StackPanel { Width = 24, Height = 24 };
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 320,
            Value = 66m,
            IsAllowClear = true,
            InnerLeftContent = "$",
            InnerRightContent = stackPanel,
            IsMotionEnabled = false
        };

        var window = Show(numericUpDown);
        try
        {
            var clear = FindSemanticControl<AvaloniaButton>(numericUpDown, ClearClass);
            clear.IsEffectivelyVisible.ShouldBeTrue();
            var clearCenter = clear.TransformToVisual(numericUpDown).ShouldNotBeNull()
                                        .Transform(new Point(0, 0)).Y + clear.Bounds.Height / 2;
            clearCenter.ShouldBe(numericUpDown.Bounds.Height / 2, 1.0);
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUINumericUpDown CreatePopulatedNumericUpDown()
    {
        return new AtomUINumericUpDown
        {
            Width = 320,
            Value = 10m,
            IsAllowClear = true,
            InnerLeftContent = "$",
            InnerRightContent = "kg",
            IsMotionEnabled = false
        };
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(typeof(AtomUINumericUpDown));
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
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
        part.StyleType.ShouldNotBeNull();
    }

    private static T FindSemanticControl<T>(AtomUINumericUpDown owner, string marker)
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
