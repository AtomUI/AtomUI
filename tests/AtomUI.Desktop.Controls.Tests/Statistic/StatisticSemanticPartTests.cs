using System.Xml.Linq;
using AtomUI.Controls.Primitives;
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
using AtomUIStatistic = AtomUI.Desktop.Controls.Statistic;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DataDisplay;

public class StatisticSemanticPartTests
{
    private const string HeaderClass = "semantic-header";
    private const string TitleClass = "semantic-title";
    private const string ContentClass = "semantic-content";
    private const string ValueClass = "semantic-value";
    private const string PrefixClass = "semantic-prefix";
    private const string SuffixClass = "semantic-suffix";
    private const string ThemePath =
        "src/AtomUI.Desktop.Controls/Statistic/Themes/StatisticTheme.axaml";

    static StatisticSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_The_Approved_Statistic_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomUIStatistic), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "content", "header", "prefix", "suffix", "title", "value"]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(descriptor, "header", HeaderClass, typeof(Border));
        AssertPart(descriptor, "title", TitleClass, typeof(ContentPresenter));
        AssertPart(descriptor, "content", ContentClass, typeof(StackPanel));
        AssertPart(descriptor, "value", ValueClass, typeof(ContentPresenter));
        AssertPart(descriptor, "prefix", PrefixClass, typeof(ContentPresenter));
        AssertPart(descriptor, "suffix", SuffixClass, typeof(ContentPresenter));
    }

    [Fact]
    public void Built_In_Template_Implements_The_Static_Marker_And_Root_Surface_Contract()
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
                   "Classes.semantic-header:Border",
                   "Classes.semantic-title:ContentPresenter",
                   "Classes.semantic-content:StackPanel",
                   "Classes.semantic-prefix:ContentPresenter",
                   "Classes.semantic-value:ContentPresenter",
                   "Classes.semantic-suffix:ContentPresenter"
               ]);
        markers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        template.Descendants().Any(static element => HasMarker(element, "semantic-root")).ShouldBeFalse();

        var frame = template.Elements().Single();
        frame.Name.LocalName.ShouldBe(nameof(DashedBorder));
        frame.Attribute("Name")?.Value.ShouldBe("Frame");
        frame.Attribute("Background")?.Value.ShouldBe("{TemplateBinding Background}");
        frame.Attribute("BorderBrush")?.Value.ShouldBe("{TemplateBinding BorderBrush}");
        frame.Attribute("BorderThickness")?.Value.ShouldBe("{TemplateBinding BorderThickness}");
        frame.Attribute("CornerRadius")?.Value.ShouldBe("{TemplateBinding CornerRadius}");
        frame.Attribute("Padding")?.Value.ShouldBe("{TemplateBinding Padding}");
        frame.Attribute("StrokeDashArray")?.Value.ShouldBe("{TemplateBinding StrokeDashArray}");

        var header = template.Descendants().Single(static element => HasMarker(element, HeaderClass));
        header.Descendants().Single(static element => HasMarker(element, TitleClass));

        var content = template.Descendants().Single(static element => HasMarker(element, ContentClass));
        content.Elements().Where(static element => element.Name.LocalName == "ContentPresenter")
               .Select(static element => (string?)element.Attribute("Name"))
               .ShouldBe(["ValuePrefixAddOn", "ContentPresenter", "ValueSuffixAddOn"]);
    }

    [Fact]
    public void Built_In_Theme_Does_Not_Consume_Semantic_Selectors()
    {
        var document = XDocument.Load(GetRepoFile(ThemePath), LoadOptions.SetLineInfo);

        document.Descendants()
                .Where(static element => element.Name.LocalName == "Style")
                .Select(static element => (string?)element.Attribute("Selector"))
                .Where(static selector => selector?.Contains(".semantic-", StringComparison.Ordinal) == true)
                .ShouldBeEmpty();
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_All_Six_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUIStatistic), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var statistic = new AtomUIStatistic
        {
            Header = "Monthly Active Users",
            Value = 93241,
            ValuePrefixAddOn = "prefix",
            ValueSuffixAddOn = "users"
        };
        statistic.Classes.Add("styled-statistic");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIStatistic>().Class("styled-statistic"));
        foreach (var partName in new[] { "header", "title", "content", "value", "prefix", "suffix" })
        {
            AddGeneratedPartStyle(ownerStyle, descriptor, partName, $"styled-{partName}");
        }
        statistic.Styles.Add(ownerStyle);

        var window = Show(statistic);
        try
        {
            FindSemanticControl<Border>(statistic, HeaderClass).Tag.ShouldBe("styled-header");
            FindSemanticControl<ContentPresenter>(statistic, TitleClass).Tag.ShouldBe("styled-title");
            FindSemanticControl<StackPanel>(statistic, ContentClass).Tag.ShouldBe("styled-content");
            FindSemanticControl<ContentPresenter>(statistic, ValueClass).Tag.ShouldBe("styled-value");
            FindSemanticControl<ContentPresenter>(statistic, PrefixClass).Tag.ShouldBe("styled-prefix");
            FindSemanticControl<ContentPresenter>(statistic, SuffixClass).Tag.ShouldBe("styled-suffix");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_Style_Projects_The_Standard_TemplatedControl_Surface()
    {
        var background = new SolidColorBrush(Color.Parse("#FFFFFF"));
        var borderBrush = new SolidColorBrush(Color.Parse("#CCCCCC"));
        var padding = new Thickness(16);
        var borderThickness = new Thickness(2);
        var cornerRadius = new CornerRadius(8);
        IReadOnlyList<double> strokeDashArray = [4d, 2d];
        var statistic = new AtomUIStatistic
        {
            Header = "Monthly Active Users",
            Value = 93241
        };
        statistic.Classes.Add("styled-root");
        statistic.Styles.Add(new Style(selector => selector.OfType<AtomUIStatistic>().Class("styled-root"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, background),
                new Setter(TemplatedControl.BorderBrushProperty, borderBrush),
                new Setter(TemplatedControl.BorderThicknessProperty, borderThickness),
                new Setter(TemplatedControl.CornerRadiusProperty, cornerRadius),
                new Setter(TemplatedControl.PaddingProperty, padding),
                new Setter(AbstractStatistic.StrokeDashArrayProperty, strokeDashArray)
            }
        });

        var window = Show(statistic);
        try
        {
            var frame = statistic.GetVisualChildren()
                                 .OfType<DashedBorder>()
                                 .Single(static border => border.Name == "Frame");
            frame.Background.ShouldBeSameAs(background);
            frame.BorderBrush.ShouldBeSameAs(borderBrush);
            frame.BorderThickness.ShouldBe(borderThickness);
            frame.CornerRadius.ShouldBe(cornerRadius);
            frame.Padding.ShouldBe(padding);
            frame.StrokeDashArray.ShouldBeSameAs(strokeDashArray);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUIStatistic));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.StyleType.ShouldBeNull();
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe($"/template/ .{selectorClass}");
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.StyleType.ShouldBe(typeof(AtomUIStatistic).Assembly.GetType(
            $"AtomUI.Theme.Styling.Statistic{char.ToUpperInvariant(name[0])}{name[1..]}Style"));
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AddGeneratedPartStyle(
        Style ownerStyle,
        ControlSemanticDescriptor descriptor,
        string partName,
        object tag)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == partName);
        var style = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull())
                                    .ShouldNotBeNull();
        style.Setters.Add(new Setter(Control.TagProperty, tag));
        ownerStyle.Children.Add(style);
    }

    private static AvaloniaWindow Show(AtomUIStatistic statistic)
    {
        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 240,
            Content = statistic
        };
        window.Show();
        statistic.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static T FindSemanticControl<T>(AtomUIStatistic statistic, string marker)
        where T : Control
    {
        return statistic.GetVisualDescendants()
                        .OfType<T>()
                        .Single(control => control.Classes.Contains(marker));
    }

    private static bool HasMarker(XElement element, string marker)
    {
        var classProperty = element.Attribute($"Classes.{marker}");
        return classProperty is not null &&
               bool.TryParse(classProperty.Value, out var isEnabled) &&
               isEnabled;
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
